import { defineConfig } from 'vitepress'

export default defineConfig({
  title: "Dequeueable",
  description: "The cloud-native ephemeral job runner for Azure Queue Storage",
  themeConfig: {
    nav: [
      { text: 'Home', link: '/' },
      { text: 'Guide', link: '/guide/getting-started' },
    ],

    sidebar: [
      {
        text: 'Guide',
        items: [
          { text: 'Getting Started', link: '/guide/getting-started' },
          { text: 'Authentication', link: '/guide/authentication' },
          { text: 'Distributed Lock', link: '/guide/distributed-lock' },
          { text: 'Timeouts', link: '/guide/timeouts' },
          { text: 'FAQ', link: '/guide/faq' },
        ]
      }
    ],

    socialLinks: [
      { icon: 'github', link: 'https://github.com/lenndewolten/Dequeueable' }
    ],

    footer: {
      message: 'Released under the MIT License.',
      copyright: 'Copyright © Lennart ten Wolde'
    }
  }
})