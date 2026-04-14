import React,{useState} from "react"
import {useNavigate} from "react-router-dom"
import {Card,CardHeader,CardTitle,CardContent} from "@/components/ui/card"
import {Button} from "@/components/ui/button"
import {Input} from "@/components/ui/input"
import {Label} from "@/components/ui/label"
import {ArrowLeft,Flame,Loader2} from "lucide-react"
import {DocumentUploader} from "@/components/ui/DocumentUploader"
import {toast} from "sonner"
import {certificateFormsApi} from "@/services/api/certificateForms"

export default function HazardousWorkerRegistration(){

const navigate=useNavigate()

const totalSteps=5
const [step,setStep]=useState(1)
const [isSubmitting,setIsSubmitting]=useState(false)

/* CATEGORY */

const [category,setCategory]=useState("")

/* PERSONAL */

const [personal,setPersonal]=useState({
workerName:"",
fatherName:"",
dob:"",
gender:"",
mobile:"",
state:"",
district:"",
address:""
})

/* IDENTITY */

const [identity,setIdentity]=useState({
aadhaarNo:"",
bpl:"No",
bplNo:"",
bhamashah:"No"
})

/* DOCUMENTS */

const [documents,setDocuments]=useState({
aadhaarCard:"",
bplCard:"",
bhamashahCard:"",
photo:""
})

/* WORK */

const [work,setWork]=useState({
serviceType:"",
joiningDate:"",
safetyTraining:"No",
ppes:"No",
hazardousCategory:""
})

/* MEDICAL */

const [medical,setMedical]=useState({
xray:false,
pft:false,
bloodTest:false
})

/* UPDATE FUNCTIONS */

const updatePersonal=(f:string,v:any)=>setPersonal(p=>({...p,[f]:v}))
const updateIdentity=(f:string,v:any)=>setIdentity(p=>({...p,[f]:v}))
const updateDocument=(f:string,v:any)=>setDocuments(p=>({...p,[f]:v}))
const updateWork=(f:string,v:any)=>setWork(p=>({...p,[f]:v}))
const updateMedical=(f:string,v:any)=>setMedical(p=>({...p,[f]:v}))

const next=()=>setStep(s=>Math.min(s+1,totalSteps))
const prev=()=>setStep(s=>Math.max(s-1,1))

const handleSubmit=async()=>{
  setIsSubmitting(true)
  try{
    const payload={
      category,
      personal,
      identity,
      documents,
      work,
      medical
    }
    const response:any=await certificateFormsApi.createHazardousWorker(payload)
    if(response?.html){
      document.open()
      document.write(response.html)
      document.close()
      return
    }
    if(response?.success!==false){
      const appId=response?.applicationId??response?.data?.applicationId
      toast.success(`Hazardous Worker registration submitted successfully!${appId?` Application ID: ${appId}`:""}`)
      navigate(-1)
    }else{
      toast.error(response?.message||"Submission failed. Please try again.")
    }
  }catch(err){
    toast.error(err instanceof Error?err.message:"Submission failed. Please try again.")
  }finally{
    setIsSubmitting(false)
  }
}

return(

<div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100">

<div className="container mx-auto px-4 py-6 space-y-6">

<Button variant="ghost" onClick={()=>navigate(-1)}>
<ArrowLeft className="h-4 w-4 mr-2"/>Back
</Button>

<Card>

<CardHeader className="bg-gradient-to-r from-primary to-primary/90 text-white">
<div className="flex items-center gap-3">
<Flame className="h-7 w-7"/>
<CardTitle>Hazardous Waste Worker Registration</CardTitle>
</div>
</CardHeader>

<div className="px-6 py-4 bg-muted/30">

<div className="flex justify-between text-sm mb-2">
<span>Step {step} of {totalSteps}</span>
<span>{Math.round((step/totalSteps)*100)}%</span>
</div>

<div className="w-full bg-muted h-2 rounded-full">
<div className="bg-primary h-2 rounded-full"
style={{width:`${(step/totalSteps)*100}%`}}/>
</div>

</div>

</Card>

{/* STEP 1 CATEGORY */}

{step===1 &&(

<StepCard title="Worker Category">

<div className="flex gap-6">

<label className="flex gap-2">
<input type="radio"
checked={category==="Hazardous"}
onChange={()=>setCategory("Hazardous")}/>
Hazardous Waste
</label>

<label className="flex gap-2">
<input type="radio"
checked={category==="E-Waste"}
onChange={()=>setCategory("E-Waste")}/>
E-Waste
</label>

</div>

</StepCard>

)}

{/* STEP 2 PERSONAL */}

{step===2 &&(

<StepCard title="Personal Details">

<TwoCol>

<Field label="Worker Name">
<Input value={personal.workerName} onChange={e=>updatePersonal("workerName",e.target.value)}/>
</Field>

<Field label="Father Name">
<Input value={personal.fatherName} onChange={e=>updatePersonal("fatherName",e.target.value)}/>
</Field>

<Field label="Date of Birth">
<Input type="date" value={personal.dob} onChange={e=>updatePersonal("dob",e.target.value)}/>
</Field>

<Field label="Gender">
<select className="w-full border rounded p-2"
value={personal.gender}
onChange={e=>updatePersonal("gender",e.target.value)}>
<option value="">Select</option>
<option>Male</option>
<option>Female</option>
</select>
</Field>

<Field label="Mobile">
<Input value={personal.mobile} onChange={e=>updatePersonal("mobile",e.target.value)}/>
</Field>

<Field label="State">
<Input value={personal.state} onChange={e=>updatePersonal("state",e.target.value)}/>
</Field>

<Field label="District">
<Input value={personal.district} onChange={e=>updatePersonal("district",e.target.value)}/>
</Field>

<Field label="Address">
<Input value={personal.address} onChange={e=>updatePersonal("address",e.target.value)}/>
</Field>

</TwoCol>

</StepCard>

)}

{/* STEP 3 IDENTITY */}

{step===3 &&(

<StepCard title="Identity & Documents">

<TwoCol>

<Field label="Do you have BPL Card?">
<div className="flex gap-4">
<label>
<input type="radio"
checked={identity.bpl==="Yes"}
onChange={()=>updateIdentity("bpl","Yes")}/> Yes
</label>
<label>
<input type="radio"
checked={identity.bpl==="No"}
onChange={()=>updateIdentity("bpl","No")}/> No
</label>
</div>
</Field>

{identity.bpl==="Yes" &&(
<>
<Field label="BPL Number">
<Input value={identity.bplNo}
onChange={e=>updateIdentity("bplNo",e.target.value)}/>
</Field>

<Field label="Upload BPL Card">
<DocumentUploader label=""
value={documents.bplCard}
onChange={url=>updateDocument("bplCard",url)}/>
</Field>
</>
)}

<Field label="Do you have Bhamashah Card?">
<div className="flex gap-4">
<label>
<input type="radio"
checked={identity.bhamashah==="Yes"}
onChange={()=>updateIdentity("bhamashah","Yes")}/> Yes
</label>
<label>
<input type="radio"
checked={identity.bhamashah==="No"}
onChange={()=>updateIdentity("bhamashah","No")}/> No
</label>
</div>
</Field>

{identity.bhamashah==="Yes" &&(
<Field label="Upload Bhamashah Card">
<DocumentUploader label=""
value={documents.bhamashahCard}
onChange={url=>updateDocument("bhamashahCard",url)}/>
</Field>
)}

<Field label="Aadhaar Number">
<Input value={identity.aadhaarNo}
onChange={e=>updateIdentity("aadhaarNo",e.target.value)}/>
</Field>

<Field label="Upload Aadhaar Card">
<DocumentUploader label=""
value={documents.aadhaarCard}
onChange={url=>updateDocument("aadhaarCard",url)}/>
</Field>

<Field label="Upload Photo">
<DocumentUploader label=""
value={documents.photo}
onChange={url=>updateDocument("photo",url)}/>
</Field>

</TwoCol>

</StepCard>

)}

{/* STEP 4 WORK + MEDICAL */}

{step===4 &&(

<StepCard title="Work Details">

<TwoCol>

<Field label="Type of Service">
<select className="w-full border rounded p-2"
value={work.serviceType}
onChange={e=>updateWork("serviceType",e.target.value)}>
<option value="">Select</option>
<option>Waste Handling</option>
<option>Waste Segregation</option>
<option>Waste Transport</option>
<option>Waste Disposal</option>
</select>
</Field>

<Field label="Joining Date">
<Input type="date"
value={work.joiningDate}
onChange={e=>updateWork("joiningDate",e.target.value)}/>
</Field>

<Field label="Safety Training">
<div className="flex gap-4">
<label>
<input type="radio"
checked={work.safetyTraining==="Yes"}
onChange={()=>updateWork("safetyTraining","Yes")}/> Yes
</label>
<label>
<input type="radio"
checked={work.safetyTraining==="No"}
onChange={()=>updateWork("safetyTraining","No")}/> No
</label>
</div>
</Field>

<Field label="PPEs Provided">
<div className="flex gap-4">
<label>
<input type="radio"
checked={work.ppes==="Yes"}
onChange={()=>updateWork("ppes","Yes")}/> Yes
</label>
<label>
<input type="radio"
checked={work.ppes==="No"}
onChange={()=>updateWork("ppes","No")}/> No
</label>
</div>
</Field>

<Field label="Hazardous Category">
<select className="w-full border rounded p-2"
value={work.hazardousCategory}
onChange={e=>updateWork("hazardousCategory",e.target.value)}>
<option value="">Select</option>
<option>Chemical Waste</option>
<option>Industrial Waste</option>
<option>Biomedical Waste</option>
<option>E-Waste</option>
</select>
</Field>

</TwoCol>

<div className="mt-6">

<h4 className="font-semibold mb-3">Medical Examination</h4>

<div className="flex gap-6">

<label className="flex gap-2">
<input type="checkbox"
checked={medical.xray}
onChange={e=>updateMedical("xray",e.target.checked)}/>
X-Ray
</label>

<label className="flex gap-2">
<input type="checkbox"
checked={medical.pft}
onChange={e=>updateMedical("pft",e.target.checked)}/>
PFT
</label>

<label className="flex gap-2">
<input type="checkbox"
checked={medical.bloodTest}
onChange={e=>updateMedical("bloodTest",e.target.checked)}/>
Blood Test
</label>

</div>

</div>

</StepCard>

)}

{/* STEP 5 PREVIEW */}

{step===5 &&(

<div className="bg-white border p-6 rounded-lg">

<table className="w-full border-collapse">

<PreviewHeader title="Category"/>
<tr>
<td className="border px-3 py-2">Worker Category</td>
<td className="border px-3 py-2">{category}</td>
</tr>

<PreviewHeader title="Personal Details"/>
{renderRows(personal)}

<PreviewHeader title="Identity"/>
{renderRows(identity)}

<PreviewHeader title="Work Details"/>
{renderRows(work)}

<PreviewHeader title="Medical"/>
{renderRows(medical)}

</table>

</div>

)}

<div className="flex justify-between">

<Button variant="outline" onClick={prev} disabled={step===1}>Previous</Button>

{step<totalSteps &&(
<Button onClick={next}>
{step===totalSteps-1?"Preview":"Next"}
</Button>
)}

{step===totalSteps &&(
<Button className="bg-green-600" onClick={handleSubmit} disabled={isSubmitting}>
{isSubmitting?(<><Loader2 className="h-4 w-4 mr-2 animate-spin"/>Submitting...</>):"Submit"}
</Button>
)}

</div>

</div>
</div>

)
}

/* COMPONENTS */

function StepCard({title,children}:any){
return(
<Card className="shadow-lg">
<CardHeader>
<CardTitle>{title}</CardTitle>
</CardHeader>
<CardContent className="space-y-6">
{children}
</CardContent>
</Card>
)
}

function TwoCol({children}:any){
return <div className="grid md:grid-cols-2 gap-4">{children}</div>
}

function Field({label,children}:any){
return(
<div className="space-y-1">
<Label>{label}</Label>
{children}
</div>
)
}

function PreviewHeader({title}:{title:string}){
return(
<tr>
<td colSpan={2} className="bg-gray-200 font-semibold px-3 py-2 border">
{title}
</td>
</tr>
)
}

function renderRows(data:any){
return Object.entries(data).map(([k,v])=>(
<tr key={k}>
<td className="bg-gray-100 px-3 py-2 border w-1/3">
{labelize(k)}
</td>
<td className="px-3 py-2 border">
{typeof v==="boolean" ? (v ? "Yes":"No") : String(v ?? "-")}
</td>
</tr>
))
}

function labelize(text:string){
return text.replace(/([A-Z])/g," $1").replace(/^./,(s)=>s.toUpperCase())
}