import axios from "axios";
import toast from "react-hot-toast";
import type { ApiError } from "../types";

export function useApiError() {
  const handleError = (error: unknown, fallbackMessage = "Ocorreu um erro inesperado") => {
    if (axios.isAxiosError<ApiError>(error)) {
      const data = error.response?.data;

      if (data?.errors) {
        const fieldErrors = Object.values(data.errors).flat();
        if (fieldErrors.length > 0) {
          fieldErrors.forEach((err) => toast.error(err));
          return;
        }
      }

      const message =
        data?.detail ||
        data?.title ||
        data?.message ||
        fallbackMessage;

      toast.error(message);
    } else {
      toast.error(fallbackMessage);
    }
  };

  return { handleError };
}
