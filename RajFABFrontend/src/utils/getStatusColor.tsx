const getStatusColor = (status: string) => {
  switch (status.toLowerCase()) {
    case "approved":
      return "bg-green-500/10 text-green-700 dark:text-green-400 hover:text-white";
    case "rejected":
      return "bg-red-500/10 text-red-700 dark:text-red-400 hover:text-white";
    case "forwarded":
      return "bg-blue-500/10 text-blue-700 dark:text-blue-400 hover:text-white";
    case "pending":
      return "bg-yellow-500/10 text-yellow-700 dark:text-yellow-400 hover:text-white";
    default:
      return "bg-muted text-muted-foreground hover:text-white";
  }
};

export default getStatusColor;
