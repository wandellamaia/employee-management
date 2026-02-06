# Employee Management Frontend

This is the frontend application for the Employee Management System, built with React, TypeScript, and Vite.

## Prerequisites

- **Node.js**: Version 20.19+ or 22.12+ is required.
- **NVM** (Optional but recommended): For managing Node.js versions.

## Getting Started

1.  **Ensure you are using the correct Node version**:
    ```bash
    nvm use 20
    # or nvm install 20 if not already installed
    ```

2.  **Install dependencies**:
    ```bash
    npm install
    ```

3.  **Run the development server**:
    ```bash
    npm run dev
    ```
    The application will be available at `http://localhost:5173`.

## Environment Configuration

Currently, the API base URL is configured in `src/api/apiClient.ts` as `http://localhost:8080`.

## Scripts

- `npm run dev`: Starts the Vite development server.
- `npm run build`: Builds the application for production.
- `npm run lint`: Runs ESLint for code quality checks.
- `npm run preview`: Previews the production build locally.

## Docker Support

You can also run the frontend using Docker through the root `docker-compose.yml`:

```bash
# From the project root
docker-compose up -d frontend
```
