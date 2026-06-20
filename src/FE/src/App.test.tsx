import React from "react";
import { render, screen } from "@testing-library/react";
import { InteractionStatus } from "@azure/msal-browser";
import App from "./App";

jest.mock("@azure/msal-react", () => ({
  useMsal: () => ({
    instance: {
      loginPopup: jest.fn(),
      logoutPopup: jest.fn(),
    },
    accounts: [],
    inProgress: "none",
  }),
}));

test("renders msal test button", () => {
  render(<App />);

  const buttonElement = screen.getByRole("button", { name: /test login msal/i });
  expect(buttonElement).toBeInTheDocument();
});
