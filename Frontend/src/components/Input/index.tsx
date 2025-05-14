import * as React from "react";
import { Eye, EyeOff } from "lucide-react";
import { cn } from "../../lib/utils";
import { InputColort, InputProps, InputVariant } from "./types";

export const Input = React.forwardRef<HTMLInputElement, InputProps>(
  (
    {
      className,
      type,
      label,
      error,
      icon,
      showPasswordToggle = false,
      variant = "outlined",
      color = "primary",
      ...props
    },
    ref
  ) => {
    const [showPassword, setShowPassword] = React.useState(false);
    const inputType = showPasswordToggle
      ? showPassword
        ? "text"
        : "password"
      : type;

    const variantStyles: Record<InputVariant, string> = {
      outlined: "border-2 border-input bg-transparent",
      filled: "border-0 bg-muted/70 hover:bg-muted focus-visible:bg-background",
      underlined:
        "rounded-none border-0 border-b-2 border-input bg-transparent px-0 focus-visible:ring-0",
    };

    const colorStyles: Record<InputColort, string> = {
      primary:
        "focus-visible:border-primary-900 focus-visible:ring-primary-900 border-primary-500",
      secondary:
        "focus-visible:border-secondary-900 focus-visible:ring-secondary-900 border-secondary-500",
      tertiary:
        "focus-visible:border-tertiary-900 focus-visible:ring-tertiary-900 border-tertiary-700",
    };

    return (
      <div className="w-full space-y-2">
        {label && (
          <label htmlFor={props.id} className="block text-sm font-medium">
            {label}
          </label>
        )}
        <div className="relative">
          {icon && (
            <div
              className={cn(
                "absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none",
                variant === "underlined" && "pl-0"
              )}
            >
              {icon}
            </div>
          )}
          <input
            type={inputType}
            className={cn(
              "flex h-12 w-full rounded-md px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 disabled:cursor-not-allowed disabled:opacity-50",
              variantStyles[variant],
              colorStyles[color],
              icon && variant !== "underlined" && "pl-10",
              icon && variant === "underlined" && "pl-7",
              showPasswordToggle && "pr-10",
              error && "border-red-500 focus-visible:ring-red-500",
              variant === "underlined" && error && "border-b-red-500",
              className
            )}
            ref={ref}
            {...props}
          />
          {showPasswordToggle && (
            <button
              type="button"
              onClick={() => setShowPassword(!showPassword)}
              className="absolute inset-y-0 right-0 flex items-center pr-3 text-gray-400 hover:text-gray-500"
              aria-label={showPassword ? "Esconder senha" : "Mostrar senha"}
            >
              {showPassword ? (
                <EyeOff className="h-4 w-4" />
              ) : (
                <Eye className="h-4 w-4" />
              )}
            </button>
          )}
        </div>
        {error && <p className="text-sm text-red-500">{error}</p>}
      </div>
    );
  }
);
