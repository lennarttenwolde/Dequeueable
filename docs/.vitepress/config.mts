import { defineConfig } from 'vitepress'

export default defineConfig({
  title: "Dequeueable",
  description: "The cloud-native ephemeral job runner for Azure Queue Storage",
  themeConfig: {
    nav: [
      { text: 'Home', link: '/' },
      { text: 'Docs', link: '/guide/getting-started' },
    ],

    sidebar: [
      {
        text: 'Guide',
        items: [
          { text: 'Getting Started', link: '/guide/getting-started' },
          { text: 'Authentication', link: '/guide/authentication' },
          { text: 'Distributed Lock', link: '/guide/distributed-lock' },
        ]
      },
      {
        text: 'Advanced',
        items: [
          { text: 'Timeouts', link: '/advanced/timeouts' },
          { text: 'Poison Queue', link: '/advanced/poison-queue' },
          { text: 'Custom Providers', link: '/advanced/custom-providers' },
          { text: 'KEDA Integration', link: '/advanced/keda' },
          { text: 'Docker', link: '/advanced/docker' },
        ]
      },
      {
        text: 'Other',
        items: [
          { text: 'FAQ', link: '/faq' },
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