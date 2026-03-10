import axios from "axios";
import toast from "react-hot-toast";

export function useApiError() {
  const handleError = (error: unknown, fallbackMessage = "Ocorreu um erro inesperado") => {
    if (axios.isAxiosError(error)) {
      const message =
        error.response?.data?.detail ||
        error.response?.data?.title ||
        error.response?.data?.message ||
        fallbackMessage;
      toast.error(message);
    } else {
      toast.error(fallbackMessage);
    }
  };

  return { handleError };
}
