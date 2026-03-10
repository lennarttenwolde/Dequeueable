---
layout: home

hero:
  name: "Dequeueable"
  text: "Cloud-native ephemeral job runner for Azure Queue Storage"
  tagline: One message. One container. Done.
  actions:
    - theme: brand
      text: Get Started →
      link: /guide/getting-started
    - theme: alt
      text: View on GitHub
      link: https://github.com/lenndewolten/Dequeueable

features:
  - icon: ⚡
    title: Ephemeral by Design
    details: Each instance starts, processes a single message, and shuts down. No idle compute, no wasted resources. Let KEDA or any external scaler drive execution.
  - icon: 🧩
    title: Zero Boilerplate
    details: Implement IQueueJob, register with AddDequeueable, call RunJobAsync. Visibility timeouts, poison queues, and error handling are all taken care of.
  - icon: 🔒
    title: Distributed Lock
    details: Built-in distributed locking via Azure Blob leases ensures only one instance processes the same scoped message at any given time.
  - icon: 🐳
    title: Container First
    details: Built for optimized alpine images. Predictable resource limits per message. Works seamlessly with Kubernetes, KEDA, and Azure Container Apps.
---