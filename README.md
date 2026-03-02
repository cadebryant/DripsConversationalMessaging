# Drips-like Conversational Messaging Workspace

This repository contains a full-stack implementation of a Drips-style messaging application, designed to help support teams identify and prioritize customer needs through automated sentiment analysis.

## Instructions for Running/Debugging Locally
* Ensure .NET 10+, Node 23+, and Angular 21 are installed.
* Set **DripsConversationalMessaging.Server** as the startup project.
* Configure to run under **https**.
* Press **F5** to run the server in Debug mode.
* In the Visual Studio terminal, run `cd dripsconversationalmessaging.client` followed by `npm start` to run the client.
* In your browser, navigate to https://localhost:49997/ to launch the user interface.

## Problem Description & User

* **The User:** Customer Support Agents and Lead Managers.
* **The Problem:** In high-volume messaging environments, support agents are often overwhelmed by a "flat" list of incoming messages. Without automated prioritization, urgent issues—such as a lead expressing extreme frustration or requesting to opt-out—can be buried under routine inquiries.
* **The Solution:** This system ingests real-time message events and uses a localized AI enrichment layer to classify the **Intent** of each message. It surfaces a "Priority Inbox" that allows agents to immediately see and respond to frustrated leads, improving retention and brand reputation.

---

## Architecture Overview

The solution is built as a cloud-native, distributed application leveraging the latest .NET and Angular ecosystems.

* **Orchestration (.NET Aspire):** Centralized orchestration for the frontend, backend, and resources. It provides built-in service discovery and a real-time observability dashboard for logs and traces.
    * Docker capablity is also supported by the inluded `Dockerfile` and `ci.yml` Github Action workflow, though the solution is designed to be run locally without requiring containerization.

* **Backend (.NET 10 Minimal API):** A lightweight, high-performance API that handles message ingestion, persistence via EF Core, and business logic orchestration.
 
* **Frontend (Angular 21):** A modern, responsive dashboard utilizing **Angular Signals** for reactive state management and a clean, task-focused UI for support agents.
 
* **Enrichment Layer (Ollama):** A local AI integration that performs asynchronous sentiment analysis on inbound message bodies to classify them as *Interested*, *Confused*, *Frustrated*, or *Opt-Out*.
 
* **Testing Suite (NUnit):** An automated testing project using **Moq** for dependency isolation and **Bogus** for generating realistic conversational data.

---

## Key Assumptions & Tradeoffs

* **In-Memory Persistence:** For the scope of this exercise, an EF Core In-Memory database is used to ensure the solution is "clone-and-run" without requiring a local SQL instance.


* **Local AI (Ollama) vs. Cloud API:** I chose to implement sentiment analysis via **Ollama** rather than a cloud API like Azure Cognitive Services.

  * *Tradeoff:* While this requires a local Ollama instance, it ensures total data privacy and eliminates per-message API costs, aligning with the **Lean** and **Ownership** values of Drips.

* **Minimal API vs. Controllers:** I opted for Minimal APIs to reduce architectural "noise" and maximize performance, assuming the service will evolve toward a microservice architecture.

* **Zoneless Angular:** The frontend uses Angular 21's zoneless reactivity to demonstrate a commitment to the latest performance standards, though it requires more disciplined Signal management than traditional Change Detection.

---

## AI Usage Summary

In alignment with Drips' values of leveraging AI-assisted tools responsibly, this project was developed using a "human-in-the-loop" AI workflow.

* **GitHub Copilot (Agent Mode):** I used Copilot as a primary pair-programmer for scaffolding the Minimal API endpoints, generating DTOs, and writing the NUnit test cases.

  * *Risk/Tradeoff:* I encountered a few "UX hangs" where the agent's terminal commands (PowerShell) timed out. I mitigated this by manually verifying the file system state and using more granular, file-specific prompts.

* **Ollama (Sentiment Analysis):** Ollama was used as the local inference engine for message classification.

  * *Appropriateness:* This was appropriate because it allowed for rapid iteration of sentiment prompts without incurring latency or costs associated with cloud-based LLMs.

---

## What I Would Improve with More Time

* **Asynchronous Messaging:** I would implement a message queue (such as Azure Service Bus) between the API ingestion and the Ollama analysis service to ensure the system remains resilient under heavy load.

* **Persistent Storage:** Migrate from In-Memory to a PostgreSQL or SQL Server container within the Aspire AppHost to support data persistence across restarts.

* **Real-time Updates:** Integrate **SignalR** to push "High Priority" alerts to the support agent's dashboard instantly without requiring a page refresh or polling.

* **Advanced Observability:** Implement Azure App Insights and custom OpenTelemetry metrics to track sentiment/intent trends over time, providing marketers with a macro-view of campaign health.
* **Infrastructure as Code:** Use Terraform or other IaC tools to automate the provisioning of cloud resources for dev, QA, UAT, and production deployment.
