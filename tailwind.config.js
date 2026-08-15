/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
        './PointingParty/Components/**/*.razor',
        './PointingParty.Client/**/*.razor'
  ],
  // The theme is an explicit choice (persisted in localStorage, stamped on
  // <html> before Blazor boots), so Tailwind's dark: variant has to follow the
  // same attribute instead of prefers-color-scheme — otherwise picking Light on
  // a dark OS would leave the Tailwind-styled screens dark.
  darkMode: ['selector', '[data-theme="dark"]'],
  theme: {
    extend: {},
  },
  // The forms plugin went with the Tailwind-styled inputs — form controls are now
  // styled by .pp-input in app.css. What Tailwind still contributes is preflight.
  plugins: [],
}
