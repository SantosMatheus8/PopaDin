export type InputVariant = "outlined" | "filled" | "underlined";

export type InputColort = "primary" | "secondary" | "tertiary";

export interface InputProps
  extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  icon?: React.ReactNode;
  showPasswordToggle?: boolean;
  variant?: InputVariant;
  color?: InputColort;
}
