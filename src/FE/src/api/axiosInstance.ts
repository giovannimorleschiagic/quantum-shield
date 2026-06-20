import axios from "axios";

const axiosInstance = axios.create({
  baseURL: process.env.REACT_APP_API_BASE_URL ?? "https://localhost:7261",
  headers: {
    "Content-Type": "application/json",
  },
});

if (process.env.REACT_APP_MOCK_API === "true") {
  // eslint-disable-next-line @typescript-eslint/no-var-requires
  const { mockAdapter } = require("../mocks/mockAdapter");
  axiosInstance.defaults.adapter = mockAdapter;
  console.info("[MockAPI] Attivo — tutte le chiamate HTTP sono simulate.");
}

export default axiosInstance;
