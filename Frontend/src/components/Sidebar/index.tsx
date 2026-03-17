import { NavLink, Link } from "react-router-dom";
import { cn } from "../../lib/utils";
import {
  Home,
  LayoutDashboard,
  Receipt,
  Tags,
  Wallet,
  Bell,
  User,
  LogOut,
  Lightbulb,
} from "lucide-react";
import { useAuth } from "../../hooks/useAuth";
import { NotificationBell } from "../NotificationBell";

const navItems = [
  { to: "/", label: "Início", icon: Home },
  { to: "/dashboard", label: "Dashboard", icon: LayoutDashboard },
  { to: "/records", label: "Registros", icon: Receipt },
  { to: "/tags", label: "Tags", icon: Tags },
  { to: "/budgets", label: "Orçamentos", icon: Wallet },
  { to: "/alerts", label: "Alertas", icon: Bell },
  { to: "/analytics", label: "Insights", icon: Lightbulb },
  { to: "/profile", label: "Perfil", icon: User },
];

export function Sidebar() {
  const { logout, user } = useAuth();

  return (
    <aside className="flex h-screen w-64 flex-col bg-secondary-900 text-white">
      <Link to="/" className="flex h-16 items-center gap-2 px-6 transition-opacity hover:opacity-80">
        <Wallet className="h-7 w-7 text-tertiary-500" />
        <span className="text-xl font-bold tracking-wide">PopaDin</span>
      </Link>

      <nav className="flex-1 space-y-1 px-3 py-4">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.to === "/"}
            className={({ isActive }) =>
              cn(
                "flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors",
                isActive
                  ? "bg-primary-700 text-white"
                  : "text-gray-300 hover:bg-secondary-700 hover:text-white"
              )
            }
          >
            <item.icon className="h-5 w-5" />
            {item.label}
          </NavLink>
        ))}
      </nav>

      <div className="border-t border-secondary-700 px-3 py-3">
        <NotificationBell />
      </div>

      <div className="border-t border-secondary-700 p-4">
        <div className="mb-3 flex items-center gap-3">
          <div className="flex h-8 w-8 shrink-0 items-center justify-center overflow-hidden rounded-full bg-secondary-700">
            {user?.profilePictureUrl ? (
              <img
                src={user.profilePictureUrl}
                alt={user.name}
                className="h-full w-full object-cover"
              />
            ) : (
              <User className="h-4 w-4 text-gray-400" />
            )}
          </div>
          <span className="truncate text-sm text-gray-400">{user?.name}</span>
        </div>
        <button
          onClick={logout}
          className="flex w-full items-center gap-2 rounded-lg px-3 py-2 text-sm text-gray-300 transition-colors hover:bg-secondary-700 hover:text-white"
        >
          <LogOut className="h-4 w-4" />
          Sair
        </button>
      </div>
    </aside>
  );
}
