import { useSearchParams, useLocation, Link } from "react-router-dom";
import { useEffect, useMemo } from "react";

const ErrorPage = () => {
  const [searchParams] = useSearchParams();
  const location = useLocation();

  const parsedError = useMemo(() => {
    const errorParam = searchParams.get("details");

    if (!errorParam) return null;
    return errorParam;
  }, [searchParams]);

  useEffect(() => {
    console.error("Error Page:", {
      path: location.pathname,
      error: parsedError,
    });
  }, [location.pathname, parsedError]);

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-100 px-4">
      <div className="text-center bg-white shadow-lg p-6 rounded-lg">
        <h1 className="text-5xl font-bold text-red-500 mb-4">
          Error
        </h1>

        <p className="text-xl text-gray-700 mb-2">
          {parsedError}
        </p>

        <Link
          to="/user"
          className="text-blue-500 hover:text-blue-700 underline"
        >
          Return to Home
        </Link>
      </div>
    </div>
  );
};

export default ErrorPage;