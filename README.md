# AnduinOS Home

[![MIT licensed](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/aiursoftweb/AnduinOS-Home/blob/master/LICENSE)
[![Pipeline stat](https://gitlab.aiursoft.com/aiursoft/AnduinOS-Home/badges/master/pipeline.svg)](https://gitlab.aiursoft.com/aiursoft/AnduinOS-Home/-/pipelines)
[![Test Coverage](https://gitlab.aiursoft.com/aiursoft/AnduinOS-Home/badges/master/coverage.svg)](https://gitlab.aiursoft.com/aiursoft/AnduinOS-Home/-/pipelines)
[![Man hours](https://manhours.aiursoft.com/r/github.com/aiursoftweb/anduinos-home.svg)](https://manhours.aiursoft.com/r/github.com/aiursoftweb/anduinos-home.html)[![Website](https://img.shields.io/website?url=https%3A%2F%2Fwww.anduinos.com%2F)](https://www.anduinos.com)
[![Docker](https://img.shields.io/docker/pulls/aiursoft/anduinos-home.svg)](https://hub.docker.com/r/aiursoft/anduinos-home)

AnduinOS-Home is the home page of AnduinOS, which is an Ubuntu based operating system. It hosts as the home page of the website, including downloading ISO files, read document and join community.

![screenshot](./screenshot.png)

## Try

Try a running AnduinOS-Home [here](https://www.anduinos.com/).

## Install with Docker

Run the installer with the desired port (omit it to choose an available port):

```bash
curl -fsSL https://raw.githubusercontent.com/aiursoftweb/anduinoshome/master/install.sh | sudo bash -s -- 8080
```

Installation starts after three seconds; press Ctrl+C to cancel. Then open `http://localhost:8080`.

## Run manually

Requirements about how to run

1. Install [.NET 10 SDK](http://dot.net/) and [Node.js](https://nodejs.org/).
2. Execute `npm install` at `wwwroot` folder to install the dependencies.
3. Execute `dotnet run` to run the app.
4. Use your browser to view [http://localhost:5000](http://localhost:5000).

## Run in Microsoft Visual Studio

1. Open the `.sln` file in the project path.
2. Press `F5` to run the app.

## Run in Docker

First, install Docker [here](https://docs.docker.com/get-docker/).

Then run the following commands in a Linux shell:

```bash
image=aiursoft/anduinos-home
appName=anduinos-home
sudo docker pull $image
sudo docker run -d --name $appName --restart unless-stopped -p 5000:5000 -v /var/www/$appName:/data $image
```

That will start a web server at `http://localhost:5000` and you can test the app.

The docker image has the following context:

| Properties  | Value                  |
|-------------|------------------------|
| Image       | aiursoft/anduinoshome  |
| Ports       | 5000                   |
| Binary path | /app                   |
| Data path   | /data                  |
| Config path | /data/appsettings.json |

## Automatic hardware content translation

Hardware reviews use the existing `HardwareTranslations` table. New reviews default to English;
write the original review once and Dotlang generates the other site languages in the background.
Device facts (brand, model, SKU, verdicts, URLs, prices) are shared rather than translated.
All 13 review text fields are translated, including capability details, configuration and image credit.

Translation is disabled by default to avoid unexpected AI calls. Configure your own
OpenAI-compatible **chat completions URL** and model through the global settings page,
using the same keys and database-backed settings mechanism as DocsViewer, or via configuration:

```json
{
  "GlobalSettings": {
    "OpenAiInstance": "https://your-ai-service.example/v1/chat/completions",
    "OpenAiLocalizationModel": "your-translation-model",
    "OpenAiApiToken": ""
  }
}
```

The dependency is `Aiursoft.Dotlang.AspNetTranslate` 10.0.70, matching DocsViewer.
The wrapper maps these global settings to Dotlang's internal `Ollama*` options.
Supply credentials through `GlobalSettings__OpenAiApiToken` rather than committing them.
The other settings also support the `GlobalSettings__` environment-variable prefix.
An empty `OpenAiInstance` disables translation; no separate Enabled flag is needed.
Database settings can be changed in the admin UI without restarting the application.
Review content is sent to the configured AI service when translation is enabled.

Saving a device queues translation without delaying the editor. Matching DocsViewer, a scheduled job starts 30 minutes
after application startup and runs every 30 minutes to backfill existing records and retry failed translations.
Administrators can also trigger **Translate hardware content** from `/Jobs` and inspect failures there.
Archived devices and incomplete reviews are skipped. Unchanged, up-to-date translations (including
manual corrections) are preserved. Editing source text increments its revision and automatically
refreshes all outdated languages; the public site shows the source while replacement translations
are pending. A language is saved only after all fields succeed, and results are discarded if the
source changes during translation. Manual edits saved during a translation are not overwritten.

## How to contribute

There are many ways to contribute to the project: logging bugs, submitting pull requests, reporting issues, and creating suggestions.

Even if you with push rights on the repository, you should create a personal fork and create feature branches there when you need them. This keeps the main repository clean and your workflow cruft out of sight.

We're also interested in your feedback on the future of this project. You can submit a suggestion or feature request through the issue tracker. To make this process more effective, we're asking that these include more information to help define them more clearly.
