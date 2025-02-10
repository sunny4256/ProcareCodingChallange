# Address Validation Service

## Introduction

The Address Validation Service is an automated application designed to validate merchant addresses using the API hosted at [https://addresses.dev-procarepay.com](https://addresses.dev-procarepay.com). The system is built to handle unreliable API behavior by implementing resilience patterns like retries, exponential backoff, and circuit breakers. The primary goal is to ensure accurate address validation even when the API faces latency or errors.

---

## Project Goals

The main goal of this service is to streamline address validation by handling common API issues, such as:

- Requests taking longer than expected.
- HTTP 5xx errors and retrying with exponential backoff.
- Preventing cascading failures using circuit breaker patterns.

Additional features include:

- Structured logging for debugging.
- Timeouts for faster failure detection.

---

## Table of Contents

1. [Overview](#overview)
2. [Installation](#installation)
3. [Usage](#usage)
4. [Logging](#logging)
5. [Error Handling](#error-handling)

---

## Overview

The Address Validation Service is a robust solution designed to validate merchant addresses by interacting with the [https://addresses.dev-procarepay.com](https://addresses.dev-procarepay.com) API. It includes resilience patterns and advanced logging mechanisms to handle real-world API challenges effectively.

### Key Features

- **Timeout:** 750ms per request to prevent long waits.
- **Automatic Retries:** Retries on HTTP 5xx errors with exponential backoff and jitter.
  - Maximum of 3 retry attempts.
- **Circuit Breaker:** Prevents cascading failures when API errors exceed thresholds.
- **Structured Logging:** Comprehensive logs for debugging and monitoring.

---

## Installation

### Prerequisites

- .NET Core (version 8.0 or higher)
- `Microsoft.Extensions.Http.Resilience` library for resilience patterns

### Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/sunny4256/ProcareCodingChallange.git
   ```
2. Navigate to the project directory:

   ```bash
   cd ProcareCodingChallange

   ```

3. Restore project dependencies:

   ```bash
   dotnet restore

   ```

4. Build the application:
   ```bash
   dotnet build
   ```

## Run the application

    ```bash
    dotnet run
    ```

## Logging

I have used Serilog logger to log to file and console. The configuration handles to create the file per each minute.

## Error-handling

Used custom expection handler to handle the 5XX errors, we use use this similar pattern to handle various exceptions
