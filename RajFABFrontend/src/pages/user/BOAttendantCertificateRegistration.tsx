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
import {validateForm,validateRequired,hasErrors,type ValidationErrors} from "@/utils/formValidation"

/* TYPES */

type Experience={
factoryName:string
factoryRegNo:string
state:string
from:string
to:string
boAttendant:string
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

export default function BOAttendantCertificateRegistration(){

const navigate=useNavigate()

const totalSteps=5
const [step,setStep]=useState(1)
const [isSubmitting,setIsSubmitting]=useState(false)
const [personErrors,setPersonErrors]=useState<ValidationErrors>({})
const [boaErrors,setBoaErrors]=useState<ValidationErrors>({})

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

/* BO ATTENDANT */

const [boAttendant,setBoAttendant]=useState({
state:"",
boAttendantNo:"",
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
boAttendant:"",
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

/* PERSON */

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
boAttendant:"",
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

/* BO ATTENDANT */

const updateBoAttendant=(field:string,value:string)=>{
setBoAttendant(prev=>({...prev,[field]:value}))
}

/* VALIDATION */

const validateCurrentStep=():boolean=>{
if(step===1){
const errs=validateForm(person,["name","fatherName","dob","address","permanentAddress","email","mobile"])
setPersonErrors(errs)
if(hasErrors(errs)){toast.error("Please fill all required fields correctly");return false}
}
if(step===2){
for(let i=0;i<experience.length;i++){
const e=experience[i]
if(!e.factoryName.trim()||!e.state.trim()||!e.from.trim()||!e.to.trim()){
toast.error(`Experience ${i+1}: Factory Name, State, From and To dates are required`)
return false
}
}
}
if(step===3){
for(let i=0;i<qualification.length;i++){
const q=qualification[i]
if(!q.degree.trim()||!q.university.trim()||!q.year.trim()){
toast.error(`Qualification ${i+1}: Degree, University and Year are required`)
return false
}
}
}
if(step===4){
const errs=validateRequired(boAttendant,["state","boAttendantNo","date"])
setBoaErrors(errs)
if(hasErrors(errs)){toast.error("Please fill all required fields");return false}
}
return true
}

/* NAV */

const next=()=>{if(validateCurrentStep()) setStep(s=>Math.min(s+1,totalSteps))}
const prev=()=>setStep(s=>Math.max(s-1,1))

/* SUBMIT */

const handleSubmit=async()=>{
  setIsSubmitting(true)
  try{
    const payload={
      person,
      boAttendantDetails:boAttendant,
      experience,
      qualification,
    }
    const response:any=await certificateFormsApi.createBOAttendant(payload)
    if(response?.html){
      document.open()
      document.write(response.html)
      document.close()
      return
    }
    if(response?.success!==false){
      const appId=response?.applicationId??response?.data?.applicationId
      toast.success(`BO Attendant Certificate registration submitted successfully!${appId?` Application ID: ${appId}`:""}`)
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
<CardTitle>BO Attendant Certificate Registration</CardTitle>
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

{/* STEP 1 */}

{step===1 &&(

<StepCard title="Personal Details">

<TwoCol>

<Field label="Name" required error={personErrors.name}>
<Input value={person.name} onChange={(e)=>updatePerson("name",e.target.value)} className={personErrors.name?"border-destructive":""}/>
</Field>
<Field label="Father Name" required error={personErrors.fatherName}>
<Input value={person.fatherName} onChange={(e)=>updatePerson("fatherName",e.target.value)} className={personErrors.fatherName?"border-destructive":""}/>
</Field>
<Field label="Date of Birth" required error={personErrors.dob}>
<Input type="date" value={person.dob} onChange={(e)=>updatePerson("dob",e.target.value)} className={personErrors.dob?"border-destructive":""}/>
</Field>
<Field label="Address" required error={personErrors.address}>
<Input value={person.address} onChange={(e)=>updatePerson("address",e.target.value)} className={personErrors.address?"border-destructive":""}/>
</Field>
<Field label="Permanent Address" required error={personErrors.permanentAddress}>
<Input value={person.permanentAddress} onChange={(e)=>updatePerson("permanentAddress",e.target.value)} className={personErrors.permanentAddress?"border-destructive":""}/>
</Field>
<Field label="Email" required error={personErrors.email}>
<Input value={person.email} onChange={(e)=>updatePerson("email",e.target.value)} className={personErrors.email?"border-destructive":""}/>
</Field>
<Field label="Mobile" required error={personErrors.mobile}>
<Input value={person.mobile} onChange={(e)=>updatePerson("mobile",e.target.value)} className={personErrors.mobile?"border-destructive":""}/>
</Field>

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
<DocumentUploader label="" value={v} onChange={(url)=>updateExperience(i,k,url)}/>
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
<DocumentUploader label="" value={v} onChange={(url)=>updateQualification(i,k,url)}/>
):(
<Input value={v} onChange={(e)=>updateQualification(i,k,e.target.value)}/>
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

{/* STEP 4 BO ATTENDANT */}

{step===4 &&(

<StepCard title="BO Attendant Certificate Details">

<TwoCol>

<Field label="State" required error={boaErrors.state}>
<Input value={boAttendant.state} onChange={(e)=>updateBoAttendant("state",e.target.value)} className={boaErrors.state?"border-destructive":""}/>
</Field>
<Field label="BO Attendant No" required error={boaErrors.boAttendantNo}>
<Input value={boAttendant.boAttendantNo} onChange={(e)=>updateBoAttendant("boAttendantNo",e.target.value)} className={boaErrors.boAttendantNo?"border-destructive":""}/>
</Field>
<Field label="Date" required error={boaErrors.date}>
<Input type="date" value={boAttendant.date} onChange={(e)=>updateBoAttendant("date",e.target.value)} className={boaErrors.date?"border-destructive":""}/>
</Field>
<Field label="Certificate">
<DocumentUploader label="" value={boAttendant.certificate} onChange={(url)=>updateBoAttendant("certificate",url)}/>
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

<PreviewHeader title="BO Attendant Certificate"/>
{renderRows(boAttendant)}

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
return (
<div className="grid md:grid-cols-2 gap-4">
{children}
</div>
)
}

function Field({label,children,error,required=false}:any){
return(
<div className="space-y-1">
<Label className={error?"text-destructive":""}>{label}{required && <span className="text-destructive ml-1">*</span>}</Label>
{children}
{error && <p className="text-xs text-destructive">{error}</p>}
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