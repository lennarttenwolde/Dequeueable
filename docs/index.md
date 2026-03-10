---
# https://vitepress.dev/reference/default-theme-home-page
layout: home

hero:
  name: "Dequeueable"
  text: "Cloud-native ephemeral job runner for Azure Queue Storage"
  tagline: Triggered by KEDA, processes a single message, and shuts down.
  actions:
    - theme: brand
      text: Get Started
      link: /guide/getting-started
    - theme: alt
      text: View on GitHub
      link: https://github.com/lenndewolten/Dequeueable

features:
  - title: Ephemeral by Design
    details: Each instance processes a single message and shuts down immediately. Scale with KEDA or any external queue scaler.
  - title: Simple API
    details: Just implement IQueueJob, register it with AddDequeueable, and call RunJobAsync. No boilerplate.
  - title: Distributed Lock
    details: Built-in distributed locking via Azure Blob leases ensures only one instance processes the same message at a time.
---
