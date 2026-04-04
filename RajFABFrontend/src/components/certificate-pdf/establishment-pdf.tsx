import React from "react";
import {
  Page,
  Text,
  View,
  Document,
  StyleSheet,
  PDFDownloadLink,
  Image,
} from "@react-pdf/renderer";
import { Button } from "../ui/button";

// Styles
const styles = StyleSheet.create({
  page: { fontSize: 10, padding: 20, fontFamily: "Helvetica" },
  headerRow: { flexDirection: "row", alignItems: "center", justifyContent: "center", gap: 10, marginBottom: 10 },
  section: { marginTop: 8, marginBottom: 2 },
  sectionTitle: { backgroundColor: "#E5E7EB", padding: 5, fontWeight: "bold", fontSize: 12 },
  row: { flexDirection: "row", borderBottom: "1 solid #ccc", paddingVertical: 4, alignItems: "center" },
  label: { width: "40%", fontWeight: "bold", paddingRight: 6 },
  value: { width: "60%" },
  bold: { fontWeight: "bold" },
  declarationTableHeader: { flexDirection: "row", backgroundColor: "#E5E7EB", border: "1 solid #000" },
  declarationHeaderCell: { width: "33.33%", padding: 6, fontSize: 9, fontWeight: "bold", textAlign: "center", borderRight: "1 solid #000" },
  declarationRow: { flexDirection: "row", border: "1 solid #000" },
  declarationCell: { width: "33.33%", padding: 6, fontSize: 9, textAlign: "center", borderRight: "1 solid #000" },
  authorityText: { fontSize: 8, textAlign: "center" },
});

// Helper row
const Row = ({ label, value }: { label: string; value: string | number | undefined }) => (
  <View style={styles.row}>
    <Text style={styles.label}>{label}</Text>
    <Text style={styles.value}>{value || "-"}</Text>
  </View>
);

// Join address dynamically
const joinAddress = (obj: any) => [
  obj?.addressLine1,
  obj?.addressLine2,
  obj?.area,
  obj?.district,
  obj?.pincode,
].filter(Boolean).join(", ");

// Main Document
const CertificateDocument = ({ data }: { data: any }) => {
  const establishment = data?.establishmentDetail;
  const mainOwner = data?.mainOwnerDetail;
  const manager = data?.managerOrAgentDetail;
  const factory = data?.factory;
  const contractors = data?.contractorDetail || [];
  const registration = data?.registrationDetail;
  
  const expiryDate = data?.endDate ? new Date(data.endDate).toLocaleDateString("en-GB") : "31/03/2030";
  const declarationDate = registration?.date ? new Date(registration.date).toLocaleDateString("en-GB") : "-";
console.log('=======', data)
  
  return (
    <Document>
      <Page style={styles.page}>
        {/* Header */}
        <View style={styles.headerRow}>
          <Image src="/Emblem_of_India.png" style={{ width: 40, height: 40 }} />
          <View style={{ flexDirection: "column", flexGrow: 1 }}>
            <Text style={{ fontSize: 18, fontWeight: "bold" }}>Form - 2</Text>
            <Text style={{ fontSize: 12 }}>(See clause (d) of sub rule (1) of rule 5)</Text>
            <Text style={{ fontSize: 12, color: "#3B82F6" }}>Certificate of Registration</Text>
          </View>
        </View>

        {/* Application Info */}
        <View style={{ ...styles.row, justifyContent: "space-between", flexDirection: "row" }}>
          <Text>
            <Text style={styles.bold}>Application Registration Number: </Text>
            {registration?.applicationRegistrationNumber || "-"}
          </Text>
          <Text>
            <Text style={styles.bold}>Date: </Text>
            {declarationDate}
          </Text>
        </View>

        <Text style={{ marginVertical: 8 }}>
          A Certificate of registration is granted under section 3 of the OSH Code, 2020 to{" "}
          <Text style={styles.bold}>{establishment?.name || "-"}</Text>
        </Text>

        {/* Establishment Details */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Details of the Establishment</Text>
          <Row label="Total Employees directly engaged" value={establishment?.totalNumberOfEmployee} />
          <Row label="Employees engaged through Contractor" value={establishment?.totalNumberOfContractEmployee} />
          <Row label="Inter State Migrant Workers" value={establishment?.totalNumberOfInterstateWorker} />
        </View>

        {/* Main Owner */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Main Owner Details</Text>
          <Row label="Name" value={mainOwner?.name} />
          <Row label="Designation" value={mainOwner?.designation} />
          <Row label="Relation" value={mainOwner?.relationType} />
          <Row label="Address" value={joinAddress(mainOwner)} />
          <Row label="Email" value={mainOwner?.email} />
          <Row label="Mobile" value={mainOwner?.mobile} />
        </View>

        {/* Manager/Agent */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Manager / Agent Details</Text>
          <Row label="Name" value={manager?.name} />
          <Row label="Designation" value={manager?.designation} />
          <Row label="Relation" value={manager?.relationType} />
          <Row label="Address" value={joinAddress(manager)} />
          <Row label="Email" value={manager?.email} />
          <Row label="Mobile" value={manager?.mobile} />
        </View>

        {/* Factory Details */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Factory Details</Text>
          <Row label="Manufacturing Type" value={factory?.manufacturingType} />
          <Row label="Manufacturing Detail" value={factory?.manufacturingDetail} />
          <Row label="Situation" value={factory?.situation} />
          <Row label="Factory Address" value={joinAddress(factory)} />
          <Row label="Maximum Workers" value={factory?.numberOfWorker} />
        </View>

        {/* Contractors */}
        {contractors.length > 0 && (
          <View style={styles.section}>
            <Text style={styles.sectionTitle}>Contractor Details</Text>
            {contractors.map((c: any, idx: number) => (
              <View key={idx} style={{ marginBottom: 4 }}>
                <Text style={{ fontWeight: "bold" }}>{c.name || "-"}</Text>
                <Row label="Work Name" value={c.nameOfWork} />
                <Row label="Max Male Workers" value={c.maxContractWorkerCountMale} />
                <Row label="Max Female Workers" value={c.maxContractWorkerCountFemale} />
                <Row label="Commencement Date" value={new Date(c.dateOfCommencement).toLocaleDateString("en-GB")} />
                <Row label="Completion Date" value={new Date(c.dateOfCompletion).toLocaleDateString("en-GB")} />
                <Row label="Address" value={joinAddress(c)} />
              </View>
            ))}
          </View>
        )}

        {/* Registration Info */}
        <View style={styles.section}>
          <Row label="Registration Fees Paid" value={registration?.amount ? `₹ ${registration.amount}` : "-"} />
        </View>

        {/* Declaration */}
        <View style={{ marginTop: 20 }}>
          <View style={styles.declarationTableHeader}>
            <Text style={styles.declarationHeaderCell}>Date of Renewal</Text>
            <Text style={styles.declarationHeaderCell}>Date of Expiry</Text>
            <Text style={styles.declarationHeaderCell}>Signature of Licensing Authority</Text>
          </View>
          <View style={styles.declarationRow}>
            <Text style={styles.declarationCell}>{declarationDate}</Text>
            <Text style={styles.declarationCell}>{expiryDate}</Text>
            <View style={styles.declarationCell}>
              {registration?.signature && <Image src={registration.signature} style={{ width: 80, height: 35, alignSelf: "center", marginBottom: 4 }} />}
              <Text style={styles.authorityText}>Chief Inspector of Factories and Boilers</Text>
              <Text style={styles.authorityText}>Rajasthan, Jaipur</Text>
            </View>
          </View>
          <Text style={{ fontSize: 9, marginTop: 8 }}>Place : {registration?.place || "-"}</Text>
          <Text style={{ fontSize: 7, marginTop: 8, textAlign: "center", color: "gray" }}>
            This is a computer generated certificate and bears scanned signature. No physical signature is required.
          </Text>
        </View>
      </Page>
    </Document>
  );
};

// PDF Download Button
export default function GeneratePDF({ data }: { data: any }) {
  return (
    <Button variant="outline">
      <PDFDownloadLink document={<CertificateDocument data={data.applicationDetails} />} fileName="establishment-certificate.pdf">
        {({ loading }) => (loading ? "Loading document..." : "Download PDF")}
      </PDFDownloadLink>
    </Button>
  );
}