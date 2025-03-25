/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
        './PointingParty/Components/**/*.razor',
        './PointingParty.Client/**/*.razor'
  ],
  theme: {
    extend: {},
  },
  plugins: [require('@tailwindcss/forms')],
  safelist: [
    { pattern: /bg-(blue|indigo|gray)-500/, variants: ['focus'] },
    { pattern: /border-(blue|indigo|gray)-200/ },
    { pattern: /text-(blue|indigo|gray)-500/ },
    { pattern: /bg-(blue|indigo|gray)-600/, variants: ['hover'] },
    { pattern: /ring-(blue|indigo|gray)-(200|500)/, variants: ['focus'] },
  ]
}
