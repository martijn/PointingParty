import {expect, Page, test} from '@playwright/test';
import {gameName} from "../lib/helpers";

test('can start a game from home', async ({page}) => {
    const game = gameName();

    await page.goto('/');

    await page.getByPlaceholder('My Pointing Party').fill(game);
    await page.getByPlaceholder('Player Name').fill('Player One');
    await page.getByRole('button', {name: 'Start game'}).click();

    await expect(page).toHaveURL(`/Game/${game}`);
    await expect(page.getByRole('heading', {name: 'Pointing Party'})).toContainText(game);
    await expect(page.getByRole('heading', {name: 'Your vote'})).toContainText('Player One');
});

test('can start a game from a game URL', async ({page}) => {
    const game = gameName();

    await page.goto(`/Game/${game}`);

    const playerNameInput = page.getByPlaceholder('Player Name');
    await playerNameInput.fill('Player Two');
    await playerNameInput.press('Enter');

    await expect(page.getByRole('heading', {name: 'Pointing Party'})).toContainText(game);
    await expect(page.getByRole('heading', {name: 'Your vote'})).toContainText('Player Two');
});

test('play with two players', async ({context}) => {
    const game = gameName();

    const pageOne = await context.newPage();
    const pageTwo = await context.newPage();

    await pageOne.goto(`/Game/${game}?PlayerName=Player%20One`);
    await pageTwo.goto(`/Game/${game}?PlayerName=Player%20Two`);

    await pageOne.getByRole('button', {name: '1', exact: true}).click();

    const pageOnePlayerOneScore = pageOne.getByRole('cell', {name: 'Player One'}).locator('+ td');
    const pageOnePlayerTwoScore = pageOne.getByRole('cell', {name: 'Player Two'}).locator('+ td');
    const pageTwoPlayerOneScore = pageTwo.getByRole('cell', {name: 'Player One'}).locator('+ td');
    const pageTwoPlayerTwoScore = pageTwo.getByRole('cell', {name: 'Player Two'}).locator('+ td');

    await expect(pageOnePlayerOneScore).toContainText('1');
    await expect(pageTwoPlayerOneScore).toContainText('Voted');

    await pageTwo.getByRole('button', {name: '2', exact: true}).click();

    await expect(pageOnePlayerTwoScore).toContainText('Voted');
    await expect(pageTwoPlayerTwoScore).toContainText('2');

    await pageOne.getByRole('button', {name: 'Show votes'}).click();

    await expect(pageOnePlayerOneScore).toContainText('1');
    await expect(pageOnePlayerTwoScore).toContainText('2');

    await expect(pageTwoPlayerOneScore).toContainText('1');
    await expect(pageTwoPlayerTwoScore).toContainText('2');

    await pageTwo.getByRole('button', {name: 'Clear votes'}).click();

    await expect(pageOnePlayerOneScore).toBeEmpty();
    await expect(pageOnePlayerTwoScore).toBeEmpty();

    await expect(pageTwoPlayerOneScore).toBeEmpty();
    await expect(pageTwoPlayerTwoScore).toBeEmpty();
});

test('shows statistics', async ({page, context}) => {
    const game = gameName();

    const joinAndVote = async (page: Page, playerName: string, vote: number | null) => {
        await page.goto(`/Game/${game}?PlayerName=${playerName}`);
        if (vote == null) return;

        await page.getByRole('button', {name: vote.toString(), exact: true}).click();
    };

    await joinAndVote(page, "Player One", 3);
    await joinAndVote(await context.newPage(), "Player Two", 5);
    await joinAndVote(await context.newPage(), "Player Three", 5);
    await joinAndVote(await context.newPage(), "Player Abstains", null);

    await expect(page.getByRole('cell', {name: 'Player Abstains'})).toBeVisible();
    await page.getByRole('button', {name: 'Show votes'}).click();

    await expect(page.getByTestId('voteCount-3')).toContainText('1');
    await expect(page.getByTestId('voteCount-5')).toContainText('2');
    await expect(page.getByTestId('average')).toContainText('4,33');
});
