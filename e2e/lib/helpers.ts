import { expect, Page, test } from "@playwright/test";

export function gameName(): string {
  return "TestGame" + Math.random().toString().substring(2, 8);
}

export async function waitForWasmInitialization(page: Page) {
  await page.waitForFunction(() => {
    const element = document.querySelector("html");
    return (
      element &&
      window
        .getComputedStyle(element)
        .getPropertyValue("--blazor-load-percentage") === "100%"
    );
  });
}
