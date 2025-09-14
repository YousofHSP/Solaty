module.exports = {
    content: [
        './**/*.razor',
        './wwwroot/**/*.html'
    ],
    theme: {
        extend: {},
    },
    plugins: [
        require('daisyui'),
        require('@tailwindcss/typography')
    ],
}
