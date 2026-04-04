import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { useManagerChangeList } from "@/hooks/api/useManagerChange";
import { Search, Eye } from "lucide-react";

export default function ManagerChangeReviewList() {
  const navigate = useNavigate();
  const { data: notices, isLoading } = useManagerChangeList();
  const [searchTerm, setSearchTerm] = useState("");

  const filteredNotices = notices?.filter((notice) => {
    const matchesSearch =
      notice.noticeNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
      notice.factoryName.toLowerCase().includes(searchTerm.toLowerCase()) ||
      notice.newManagerName.toLowerCase().includes(searchTerm.toLowerCase());
    return matchesSearch;
  });

  if (isLoading) return <div className="container mx-auto p-6">Loading...</div>;

  return (
    <div className="container mx-auto p-6 space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Manager Change Notices</h1>
        <p className="text-muted-foreground">View submitted manager change notices</p>
      </div>

      <Card>
        <CardHeader>
          <div className="flex items-center gap-4">
            <div className="flex-1 relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search by notice number, factory, or manager name..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10"
              />
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {filteredNotices?.map((notice) => (
              <div
                key={notice.id}
                className="p-4 border rounded-lg hover:bg-accent cursor-pointer transition-colors"
                onClick={() => navigate(`/admin/manager-change-review/${notice.id}`)}
              >
                <div className="flex justify-between items-start">
                  <div className="space-y-1 flex-1">
                    <div className="flex items-center gap-3">
                      <h3 className="font-semibold">{notice.noticeNumber}</h3>
                      <Badge variant="secondary">{notice.status}</Badge>
                    </div>
                    <p className="text-sm font-medium text-muted-foreground">{notice.factoryName}</p>
                    <p className="text-sm">
                      {notice.outgoingManagerName} → {notice.newManagerName}
                    </p>
                    <p className="text-xs text-muted-foreground">
                      Submitted: {new Date(notice.submittedAt).toLocaleDateString()}
                    </p>
                  </div>
                  <Button variant="outline" size="sm">
                    <Eye className="h-4 w-4 mr-2" />
                    View Details
                  </Button>
                </div>
              </div>
            ))}
            {filteredNotices?.length === 0 && (
              <div className="text-center py-8 text-muted-foreground">
                No manager change notices found
              </div>
            )}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
