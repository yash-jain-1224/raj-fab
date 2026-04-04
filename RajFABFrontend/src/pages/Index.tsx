import Navigation from "@/components/ui/navigation";
import HeroSection from "@/components/ui/hero-section";
import ServicesSection from "@/components/ui/services-section";
import UserPortals from "@/components/ui/user-portals";
import FeaturesSection from "@/components/ui/features-section";
import Footer from "@/components/ui/footer";

const Index = () => {
  return (
    <div className="min-h-screen bg-background">
      <Navigation />
      <main>
        <HeroSection />
        <ServicesSection />
        <UserPortals />
        <FeaturesSection />
      </main>
      <Footer />
    </div>
  );
};

export default Index;
