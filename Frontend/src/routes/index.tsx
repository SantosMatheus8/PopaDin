import { createBrowserRouter } from "react-router-dom";
import App from "../App";
import PrivateRoute from "../components/PrivateRoutes";
import { Login } from "../pages/Login";
import { Dashboard } from "../pages/dashboard";

export const router = createBrowserRouter([
  {
    path: "/",
    element: <App />,
    children: [
      { path: "login", element: <Login /> },
      {
        path: "dashboard",
        element: (
          <PrivateRoute>
            <Dashboard />
          </PrivateRoute>
        ),
      },
    ],
  },
]);
