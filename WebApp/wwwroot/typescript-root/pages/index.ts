const greeting = document.querySelector<HTMLHeadingElement>("#greeting");
const message = document.querySelector<HTMLParagraphElement>("#message");
const helloButton = document.querySelector<HTMLButtonElement>("#hello-button");
const databaseReadinessStatus = document.querySelector<HTMLParagraphElement>("#database-readiness-status");
const databaseReadinessButton = document.querySelector<HTMLButtonElement>("#database-readiness-button");

if (
    greeting === null
    || message === null
    || helloButton === null
    || databaseReadinessStatus === null
    || databaseReadinessButton === null
) {
    throw new Error("Required page elements could not be found.");
}

const readinessStatus = databaseReadinessStatus;
const readinessButton = databaseReadinessButton;

// TEMPORARY: Replace with WebApp's configured runtime API client during P8.
const temporaryDatabaseReadinessEndpointUrl = "https://localhost:7144/api/v1/_temporary/database-readiness";

interface TemporaryDatabaseReadinessResponse {
    isReady: boolean;
}

message.textContent = "TypeScript loaded successfully.";

helloButton.addEventListener("click", () => {
    greeting.textContent = "Hello from TypeScript!";
});

readinessButton.addEventListener("click", () => {
    void checkDatabaseReadinessAsync();
});

void checkDatabaseReadinessAsync();

async function checkDatabaseReadinessAsync(): Promise<void> {
    readinessButton.disabled = true;
    readinessStatus.textContent = "Checking the database readiness endpoint…";

    try {
        const response = await fetch(temporaryDatabaseReadinessEndpointUrl);
        const responseBody: unknown = await response.json();

        if (!isTemporaryDatabaseReadinessResponse(responseBody)) {
            throw new Error("The database readiness endpoint returned an unexpected response.");
        }

        readinessStatus.textContent = responseBody.isReady
            ? "Database is ready."
            : "Database is unavailable.";
    } catch {
        readinessStatus.textContent = "Database readiness could not be checked.";
    } finally {
        readinessButton.disabled = false;
    }
}

function isTemporaryDatabaseReadinessResponse(
    value: unknown
): value is TemporaryDatabaseReadinessResponse {
    if (typeof value !== "object" || value === null || !("isReady" in value)) {
        return false;
    }

    return typeof value.isReady === "boolean";
}
