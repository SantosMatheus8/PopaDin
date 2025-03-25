import { ButtonProps } from ".";
import { cn } from "../../lib/utils";

export const Button = ({
  variant = "primary",
  size = "md",
  className = "",
  children,
  icon,
  iconPosition = "left",
  disabled = false,
  ...rest
}: ButtonProps) => {
  const baseStyles =
    "inline-flex items-center justify-center rounded-md font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-offset-2 disabled:pointer-events-none disabled:bg-disabled cursor-pointer";

  const variantStyles = {
    primary: "bg-primary-500 text-white hover:bg-primary-700",
    secondary: "bg-secondary-500 text-white hover:bg-secondary-700",
    tertiary: "bg-tertiary-500 text-white hover:bg-tertiary-700",
    error: "bg-error text-white hover:bg-error/80",
    outline: "border border-input bg-background hover:opacity-50",
    link: "text-primary underline-offset-4 hover:underline",
  };

  const sizeStyles = {
    sm: "h-8 px-3 text-xs",
    md: "h-10 px-4 py-2",
    lg: "h-12 px-6 py-3 text-lg",
  };

  return (
    <button
      className={cn(
        baseStyles,
        variantStyles[variant],
        sizeStyles[size],
        className
      )}
      disabled={disabled}
      {...rest}
    >
      {icon && iconPosition === "left" && <span className="mr-2">{icon}</span>}
      {children}
      {icon && iconPosition === "right" && <span className="ml-2">{icon}</span>}
    </button>
  );
};
