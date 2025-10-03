/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./src/**/*.{html,ts}"],
  theme: {
    extend: {
      colors: {
        brand: { DEFAULT: "#2563eb" },
        container: { center: true, padding: "1rem" },
        boxShadow: { card: "0 1px 2px rgba(16,24,40,.06), 0 1px 3px rgba(16,24,40,.1)" }
      }
    }
  },
  plugins: []
};
