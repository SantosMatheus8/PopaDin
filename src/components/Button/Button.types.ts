export type ButtonProps = {
  variant?: "primary" | "secondary" | "tertiary" | "outline" | "error" | "link";
  size?: "sm" | "md" | "lg";
  className?: string;
  children: React.ReactNode;
  icon?: React.ReactNode;
  iconPosition?: "left" | "right";
  disabled?: boolean;
} & React.ButtonHTMLAttributes<HTMLButtonElement>;
