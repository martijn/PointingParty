import { test, expect } from '@playwright/test';
import { gameName } from "../lib/helpers";

test('can start a game from home', async ({ page }) => {
  const game = gameName();
  
  await page.goto('https://pointingparty.com/');

  // Click the get started link.
  await page.getByPlaceholder('My Pointing Party').fill(game);
  await page.getByPlaceholder('Player Name').fill('Player One');

  await page.getByRole('button', { name: 'Start game' }).click();

  await expect(page).toHaveURL('https://pointingparty.com/Game/' + game);
  
  await expect(page.getByRole('heading', { name: 'Pointing Party' } )).toContainText(game);
  await expect(page.getByRole('heading', { name: 'Your vote' })).toContainText('Player One');
});

test('can start a game from a game URL', async ({ page }) => {
  const game = gameName();
  
  await page.goto('https://pointingparty.com/Game/' + game);

  // Click the get started link.
  await page.getByPlaceholder('Player Name').fill('Player Two');

  await page.getByRole('button', { name: 'Enter game' }).click();

  await expect(page.getByRole('heading', { name: 'Pointing Party' } )).toContainText(game);
  await expect(page.getByRole('heading', { name: 'Your vote' })).toContainText('Player Two');
});

test('two players see eachother', async ({ context }) => {
  const game = gameName();

  const pageOne = await context.newPage();
  const pageTwo = await context.newPage();
  
  await pageOne.goto('https://pointingparty.com/Game/' + game + '?PlayerName=Player%20One');
  await pageTwo.goto('https://pointingparty.com/Game/' + game + '?PlayerName=Player%20Two');
  
  await pageOne.getByRole('button', { name: '1', exact: true }).click();

  await expect(pageOne.getByRole('cell', { name: 'Player One' }).locator('+ td')).toContainText('1');
  await expect(pageTwo.getByRole('cell', { name: 'Player One' }).locator('+ td')).toContainText('Voted');
  
  await pageTwo.getByRole('button', { name: '2', exact: true }).click();

  await expect(pageOne.getByRole('cell', { name: 'Player Two' }).locator('+ td')).toContainText('Voted');
  await expect(pageTwo.getByRole('cell', { name: 'Player Two' }).locator('+ td')).toContainText('2');
  
  await pageOne.getByRole('button', { name: 'Show votes' }).click();
  
  await expect(pageOne.getByRole('cell', { name: 'Player One' }).locator('+ td')).toContainText('1');
  await expect(pageOne.getByRole('cell', { name: 'Player Two' }).locator('+ td')).toContainText('2');

  await expect(pageTwo.getByRole('cell', { name: 'Player One' }).locator('+ td')).toContainText('1');
  await expect(pageTwo.getByRole('cell', { name: 'Player Two' }).locator('+ td')).toContainText('2');
  
  await pageTwo.getByRole('button', { name: 'Clear votes' }).click();
  
  await expect(pageOne.getByRole('cell', { name: 'Player One' }).locator('+ td')).toBeEmpty();
  await expect(pageOne.getByRole('cell', { name: 'Player Two' }).locator('+ td')).toBeEmpty();

  await expect(pageTwo.getByRole('cell', { name: 'Player One' }).locator('+ td')).toBeEmpty();
  await expect(pageTwo.getByRole('cell', { name: 'Player Two' }).locator('+ td')).toBeEmpty();
  
  await pageOne.close()
  await pageTwo.close()
});
