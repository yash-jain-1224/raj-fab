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

/* TYPES */

type Experience={
factoryName:string
factoryRegNo:string
state:string
from:string
to:string
foAttendant:string
boilerInspection:string
expDays:string
document:string
}

type Qualification={
degree:string
branch:string
university:string
state:string
year:string
percent:string
document:string
}

export default function FOAttendantCertificateRegistration(){

const navigate=useNavigate()

const totalSteps=5
const [step,setStep]=useState(1)
const [isSubmitting,setIsSubmitting]=useState(false)

/* PERSONAL */

const [person,setPerson]=useState({
name:"",
fatherName:"",
dob:"",
address:"",
permanentAddress:"",
email:"",
mobile:""
})

/* FO ATTENDANT */

const [foAttendant,setFoAttendant]=useState({
state:"",
foAttendantNo:"",
date:"",
certificate:""
})

/* EXPERIENCE */

const [experience,setExperience]=useState<Experience[]>([{
factoryName:"",
factoryRegNo:"",
state:"",
from:"",
to:"",
foAttendant:"",
boilerInspection:"",
expDays:"",
document:""
}])

/* QUALIFICATION */

const [qualification,setQualification]=useState<Qualification[]>([{
degree:"",
branch:"",
university:"",
state:"",
year:"",
percent:"",
document:""
}])

/* PERSON UPDATE */

const updatePerson=(field:string,value:string)=>{
setPerson(prev=>({...prev,[field]:value}))
}

/* EXPERIENCE */

const updateExperience=(i:number,field:string,value:string)=>{
const arr=[...experience]
arr[i]={...arr[i],[field]:value}
setExperience(arr)
}

const addExperience=()=>{
setExperience([...experience,{
factoryName:"",
factoryRegNo:"",
state:"",
from:"",
to:"",
foAttendant:"",
boilerInspection:"",
expDays:"",
document:""
}])
}

const removeExperience=(i:number)=>{
setExperience(experience.filter((_,index)=>index!==i))
}

/* QUALIFICATION */

const updateQualification=(i:number,field:string,value:string)=>{
const arr=[...qualification]
arr[i]={...arr[i],[field]:value}
setQualification(arr)
}

const addQualification=()=>{
setQualification([...qualification,{
degree:"",
branch:"",
university:"",
state:"",
year:"",
percent:"",
document:""
}])
}

const removeQualification=(i:number)=>{
setQualification(qualification.filter((_,index)=>index!==i))
}

/* FO ATTENDANT UPDATE */

const updateFoAttendant=(field:string,value:string)=>{
setFoAttendant(prev=>({...prev,[field]:value}))
}

/* NAV */

const next=()=>setStep(s=>Math.min(s+1,totalSteps))
const prev=()=>setStep(s=>Math.max(s-1,1))

/* SUBMIT */

const handleSubmit=async()=>{
  setIsSubmitting(true)
  try{
    const payload={
      person,
      foAttendantDetails:foAttendant,
      experience,
      qualification,
    }
    const response:any=await certificateFormsApi.createFOAttendant(payload)
    if(response?.html){
      document.open()
      document.write(response.html)
      document.close()
      return
    }
    if(response?.success!==false){
      const appId=response?.applicationId??response?.data?.applicationId
      toast.success(`FO Attendant Certificate registration submitted successfully!${appId?` Application ID: ${appId}`:""}`)
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
<ArrowLeft className="h-4 w-4 mr-2"/>
Back
</Button>

<Card>

<CardHeader className="bg-gradient-to-r from-primary to-primary/90 text-white">
<div className="flex items-center gap-3">
<Flame className="h-7 w-7"/>
<CardTitle>FO Attendant Certificate Registration</CardTitle>
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

{/* STEP 1 PERSONAL */}

{step===1 &&(

<StepCard title="Personal Details">

<TwoCol>

{Object.entries(person).map(([k,v])=>(
<Field key={k} label={labelize(k)}>
<Input value={v} onChange={(e)=>updatePerson(k,e.target.value)}/>
</Field>
))}

</TwoCol>

</StepCard>

)}

{/* STEP 2 EXPERIENCE */}

{step===2 &&(

<StepCard title="Experience">

{experience.map((exp,i)=>(

<div key={i} className="border p-4 rounded-lg space-y-4">

<div className="flex justify-between">
<h4 className="font-semibold">Experience {i+1}</h4>

{experience.length>1 &&(
<Button variant="destructive" size="sm" onClick={()=>removeExperience(i)}>
Remove
</Button>
)}

</div>

<TwoCol>

{Object.entries(exp).map(([k,v])=>(
<Field key={k} label={labelize(k)}>

{k==="document"?(
<DocumentUploader
label=""
value={v}
onChange={(url)=>updateExperience(i,k,url)}
/>
):(

<Input
type={k==="from"||k==="to"?"date":"text"}
value={v}
onChange={(e)=>updateExperience(i,k,e.target.value)}
/>

)}

</Field>
))}

</TwoCol>

</div>

))}

<Button onClick={addExperience}>
Add Experience
</Button>

</StepCard>

)}

{/* STEP 3 QUALIFICATION */}

{step===3 &&(

<StepCard title="Qualification">

{qualification.map((q,i)=>(

<div key={i} className="border p-4 rounded-lg space-y-4">

<div className="flex justify-between">
<h4 className="font-semibold">Qualification {i+1}</h4>

{qualification.length>1 &&(
<Button variant="destructive" size="sm" onClick={()=>removeQualification(i)}>
Remove
</Button>
)}

</div>

<TwoCol>

{Object.entries(q).map(([k,v])=>(
<Field key={k} label={labelize(k)}>

{k==="document"?(
<DocumentUploader
label=""
value={v}
onChange={(url)=>updateQualification(i,k,url)}
/>
):(
<Input
value={v}
onChange={(e)=>updateQualification(i,k,e.target.value)}
/>
)}

</Field>
))}

</TwoCol>

</div>

))}

<Button onClick={addQualification}>
Add Qualification
</Button>

</StepCard>

)}

{/* STEP 4 FO ATTENDANT */}

{step===4 &&(

<StepCard title="FO Attendant Certificate Details">

<TwoCol>

<Field label="State">
<Input value={foAttendant.state} onChange={(e)=>updateFoAttendant("state",e.target.value)}/>
</Field>

<Field label="FO Attendant No">
<Input value={foAttendant.foAttendantNo} onChange={(e)=>updateFoAttendant("foAttendantNo",e.target.value)}/>
</Field>

<Field label="Date">
<Input type="date" value={foAttendant.date} onChange={(e)=>updateFoAttendant("date",e.target.value)}/>
</Field>

<Field label="Certificate">
<DocumentUploader
label=""
value={foAttendant.certificate}
onChange={(url)=>updateFoAttendant("certificate",url)}
/>
</Field>

</TwoCol>

</StepCard>

)}

{/* STEP 5 PREVIEW */}

{step===5 &&(

<div className="bg-white border p-6 rounded-lg">

<table className="w-full border-collapse">

<PreviewHeader title="Personal Details"/>
{renderRows(person)}

<PreviewHeader title="Experience"/>
{experience.map((exp,i)=>(
<React.Fragment key={i}>
<tr>
<td colSpan={2} className="bg-gray-100 font-semibold px-3 py-2 border">
Experience {i+1}
</td>
</tr>
{renderRows(exp)}
</React.Fragment>
))}

<PreviewHeader title="Qualification"/>
{qualification.map((q,i)=>(
<React.Fragment key={i}>
<tr>
<td colSpan={2} className="bg-gray-100 font-semibold px-3 py-2 border">
Qualification {i+1}
</td>
</tr>
{renderRows(q)}
</React.Fragment>
))}

<PreviewHeader title="FO Attendant Certificate"/>
{renderRows(foAttendant)}

</table>

</div>

)}

<div className="flex justify-between">

<Button variant="outline" onClick={prev} disabled={step===1}>
Previous
</Button>

{step<totalSteps &&(
<Button onClick={next}>
{step===totalSteps-1?"Preview":"Next"}
</Button>
)}

{step===totalSteps &&(
<Button className="bg-green-600" onClick={handleSubmit} disabled={isSubmitting}>
{isSubmitting?(
  <><Loader2 className="h-4 w-4 mr-2 animate-spin"/>Submitting...</>
):"Submit"}
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
<span className="text-sm text-gray-700">
{v ? String(v) : "-"}
</span>
</td>
</tr>
))
}

function labelize(text:string){
return text
.replace(/([A-Z])/g," $1")
.replace(/^./,(s)=>s.toUpperCase())
}