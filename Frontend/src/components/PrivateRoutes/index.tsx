import { Navigate } from "react-router-dom";

interface Props {
  children: React.ReactNode;
}

export default function PrivateRoute({ children }: Props) {
  const isAuthenticated = false;

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
}
