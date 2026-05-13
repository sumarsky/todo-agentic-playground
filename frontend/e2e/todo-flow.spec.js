import { expect, test } from '@playwright/test';

test('user completes todo flow against real backend API', async ({ page }) => {
  const suffix = Date.now();
  const writeTitle = `Write system tests ${suffix}`;
  const updatedTitle = `Write real system tests ${suffix}`;
  const shipTitle = `Ship UI ${suffix}`;
  const docsTitle = `Fix docs ${suffix}`;

  await page.goto('/');

  const titleInput = page.getByLabel('Todo title');
  await titleInput.fill(writeTitle);
  await page.getByRole('button', { name: 'Add todo' }).click();
  await expect(page.getByText(writeTitle)).toBeVisible();

  await titleInput.fill(shipTitle);
  await page.getByRole('button', { name: 'Add todo' }).click();
  await expect(page.getByText(shipTitle)).toBeVisible();

  await titleInput.fill(docsTitle);
  await page.getByRole('button', { name: 'Add todo' }).click();
  await expect(page.getByText(docsTitle)).toBeVisible();

  await page.getByLabel('Search todos').fill('Write system');
  await expect(page.getByText(writeTitle)).toBeVisible();
  await expect(page.getByText(shipTitle)).toBeHidden();

  await page.getByLabel('Search todos').fill('');
  await expect(page.getByText(shipTitle)).toBeVisible();

  await page.getByRole('button', { name: `Edit ${writeTitle}` }).click();
  await page.getByLabel('Edit todo title').fill(updatedTitle);
  await page.getByRole('button', { name: 'Save todo' }).click();
  await expect(page.getByText(updatedTitle)).toBeVisible();

  await page.getByRole('checkbox', { name: `Mark ${updatedTitle} complete` }).click();
  await page.getByRole('button', { name: 'Show completed todos' }).click();
  await expect(page.getByText(updatedTitle)).toBeVisible();
  await expect(page.getByText(shipTitle)).toBeHidden();

  await page.getByRole('button', { name: 'Show completed todos' }).click();
  await expect(page.getByText(shipTitle)).toBeVisible();

  await page.getByRole('button', { name: `Delete ${shipTitle}` }).click();
  await expect(page.getByText(shipTitle)).toBeHidden();

  await page.getByRole('checkbox', { name: `Select ${updatedTitle}` }).check();
  await page.getByRole('checkbox', { name: `Select ${docsTitle}` }).check();
  await page.getByRole('button', { name: 'Delete selected (2)' }).click();

  await expect(page.getByText(updatedTitle)).toBeHidden();
  await expect(page.getByText(docsTitle)).toBeHidden();
});
