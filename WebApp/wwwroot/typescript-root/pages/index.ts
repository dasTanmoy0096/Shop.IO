const greeting = document.querySelector<HTMLHeadingElement>("#greeting");
const message = document.querySelector<HTMLParagraphElement>("#message");
const helloButton = document.querySelector<HTMLButtonElement>("#hello-button");

if (greeting === null || message === null || helloButton === null) {
    throw new Error("Required page elements could not be found.");
}

message.textContent = "TypeScript loaded successfully.";

helloButton.addEventListener("click", () => {
    greeting.textContent = "Hello from TypeScript!";
});
