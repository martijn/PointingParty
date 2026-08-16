import { expect, Page, test } from "@playwright/test";
import { gameName, waitForWasmInitialization } from "../lib/helpers";

test("can start a game from home", async ({ page }) => {
  const game = gameName();

  await page.goto("/");

  await page.getByPlaceholder("My Pointing Party").fill(game);
  await page.getByPlaceholder("Player Name").fill("Player One");
  await page.getByRole("button", { name: "Start game" }).click();

  await expect(page).toHaveURL(`/Game/${game}`);
  await expect(page.getByTestId("player-row-Player One")).toContainText("you");
});

test("can start a game from a game URL", async ({ page }) => {
  const game = gameName();

  await page.goto(`/Game/${game}`);

  // Not great pactice, but Playwright is so fast with entering the player name, the wasm hydration overwrites it
  await waitForWasmInitialization(page);

  await page.getByPlaceholder("Player Name").fill("Player Two");
  await page.getByRole("button", { name: "Enter game" }).click();

  await expect(page.getByTestId("player-row-Player Two")).toContainText("you");
});

test("play with two players", async ({ context }) => {
  const game = gameName();

  const pageOne = await context.newPage();
  const pageTwo = await context.newPage();

  await pageOne.goto(`/Game/${game}?PlayerName=Player%20One`);
  await pageTwo.goto(`/Game/${game}?PlayerName=Player%20Two`);

  await pageOne.getByRole("button", { name: "1", exact: true }).click();

  const pageOnePlayerOneScore = pageOne.getByTestId("vote-for-Player One");
  const pageOnePlayerTwoScore = pageOne.getByTestId("vote-for-Player Two");
  const pageTwoPlayerOneScore = pageTwo.getByTestId("vote-for-Player One");
  const pageTwoPlayerTwoScore = pageTwo.getByTestId("vote-for-Player Two");

  // Votes stay hidden until the reveal — the roster only says who is ready.
  await expect(pageOnePlayerOneScore).toBeEmpty();
  await expect(pageOne.getByTestId("state-for-Player One")).toContainText("ready");
  await expect(pageTwo.getByTestId("state-for-Player One")).toContainText("ready");

  await pageTwo.getByRole("button", { name: "2", exact: true }).click();

  await expect(pageOne.getByTestId("state-for-Player Two")).toContainText("ready");
  await expect(pageTwo.getByTestId("state-for-Player Two")).toContainText("ready");

  await pageOne.getByRole("button", { name: "Reveal votes" }).click();

  await expect(pageOnePlayerOneScore).toContainText("1");
  await expect(pageOnePlayerTwoScore).toContainText("2");

  await expect(pageTwoPlayerOneScore).toContainText("1");
  await expect(pageTwoPlayerTwoScore).toContainText("2");

  await pageTwo.getByRole("button", { name: "Next story" }).click();

  await expect(pageOnePlayerOneScore).toBeEmpty();
  await expect(pageOnePlayerTwoScore).toBeEmpty();

  await expect(pageTwoPlayerOneScore).toBeEmpty();
  await expect(pageTwoPlayerTwoScore).toBeEmpty();
});

test("counts rounds", async ({ page }) => {
  const game = gameName();

  await page.goto(`/Game/${game}?PlayerName=Player%20One`);

  await expect(page.getByTestId("round")).toHaveText("Round 1");

  await page.getByRole("button", { name: "New round" }).click();

  await expect(page.getByTestId("round")).toHaveText("Round 2");
});

test("shows the verdict and the median", async ({ page, context }) => {
  const game = gameName();

  const joinAndVote = async (page: Page, playerName: string, vote: number | null) => {
    await page.goto(`/Game/${game}?PlayerName=${playerName}`);
    if (vote == null) return;

    await page.getByRole("button", { name: vote.toString(), exact: true }).click();
  };

  await joinAndVote(page, "Player One", 3);
  await joinAndVote(await context.newPage(), "Player Two", 5);
  await joinAndVote(await context.newPage(), "Player Three", 5);
  await joinAndVote(await context.newPage(), "Player Abstains", null);

  await expect(page.getByTestId("player-row-Player Abstains")).toBeVisible();
  await page.getByRole("button", { name: "Reveal votes" }).click();

  // 3, 5, 5 — the abstention is ignored, so the median is 5 and 5/3 counts as tight.
  await expect(page.getByTestId("median")).toContainText("5");
  await expect(page.getByTestId("verdict")).toContainText("Tight");
});
