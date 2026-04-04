import { Outlet } from "react-router-dom";


export default function UserLayoutNoSidebar() {
  return (
    <div className="min-h-screen flex w-full bg-background">
      <main className="flex-1 overflow-auto">
        <div className="container mx-auto p-6">
          <Outlet />
        </div>
      </main>
    </div>
  );
}
