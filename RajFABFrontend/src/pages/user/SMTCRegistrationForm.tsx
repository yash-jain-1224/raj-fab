import React,{useState} from "react"
import {useNavigate} from "react-router-dom"
import {Card,CardHeader,CardTitle,CardContent} from "@/components/ui/card"
import {Button} from "@/components/ui/button"
import {Input} from "@/components/ui/input"
import {Label} from "@/components/ui/label"
import {ArrowLeft,Flame,Loader2} from "lucide-react"
import {DocumentUploader} from "@/components/ui/DocumentUploader"
import {toast} from "sonner"
import {BaseApiService} from "@/services/api/base"

class SmtcApiService extends BaseApiService {
  async create(dto: unknown): Promise<any> {
    return this.request<any>('/SMTC/create', {
      method: 'POST',
      body: JSON.stringify(dto),
    });
  }
}
const smtcApi = new SmtcApiService();

/* TYPES */

type Education={
educationType:"qualification"|"engineering"
course:string
degree:string
universityCollege:string
passingYear:string
specialization:string
}

type Trainer={
trainerName:string
totalYearsExperience:string
mobile:string
educationDetails:Education[]
photoPath:string
degreeDocumentPath:string
}

/* COMPONENT */

export default function SMTCRegistrationForm(){

const navigate=useNavigate()

const totalSteps=4
const [step,setStep]=useState(1)
const [isSubmitting,setIsSubmitting]=useState(false)

/* STEP1 FACTORY */

const [factory,setFactory]=useState({
factoryRegistrationNo:"",
factoryName:"",
factoryAddress:"",
factoryArea:"",
factoryDistrict:"",
factoryManagerName:"",
factoryManagerMobile:""
})

/* STEP2 FACILITY */

const [facility,setFacility]=useState({
trainingCenterAvailable:"",
seatingCapacity:"",
audioVideoFacility:"",
comments:"",
trainingCenterPhoto:""
})

/* STEP3 TRAINERS */

const [trainers,setTrainers]=useState<Trainer[]>([{
trainerName:"",
totalYearsExperience:"",
mobile:"",
educationDetails:[
{educationType:"qualification",course:"",degree:"",universityCollege:"",passingYear:"",specialization:""},
{educationType:"engineering",course:"",degree:"",universityCollege:"",passingYear:"",specialization:""}  
],
photoPath:"",
degreeDocumentPath:""
}])

/* FACTORY UPDATE */

const updateFactory=(field:string,value:string)=>{
setFactory(prev=>({...prev,[field]:value}))
}

/* FACILITY UPDATE */

const updateFacility=(field:string,value:string)=>{
setFacility(prev=>({...prev,[field]:value}))
}

/* TRAINER UPDATE */

const updateTrainer=(ti:number,field:string,value:string)=>{
const arr=[...trainers]
arr[ti]={...arr[ti],[field]:value}
setTrainers(arr)
}

/* EDUCATION UPDATE */

const updateEducation=(ti:number,ei:number,field:string,value:string)=>{
const arr=[...trainers]
arr[ti].educationDetails[ei]={
...arr[ti].educationDetails[ei],
[field]:value
}
setTrainers(arr)
}

/* ADD TRAINER */

const addTrainer=()=>{
setTrainers([...trainers,{
trainerName:"",
totalYearsExperience:"",
mobile:"",

educationDetails:[
{educationType:"qualification",course:"",degree:"",universityCollege:"",passingYear:"",specialization:""},
{educationType:"engineering",course:"",degree:"",universityCollege:"",passingYear:"",specialization:""}
],
photoPath:"",
degreeDocumentPath:""
}])
}

const removeTrainer=(ti:number)=>{
setTrainers(trainers.filter((_,i)=>i!==ti))
}

/* NAV */

const next=()=>setStep(s=>Math.min(s+1,totalSteps))
const prev=()=>setStep(s=>Math.max(s-1,1))

/* SUBMIT */

const handleSubmit=async()=>{
  setIsSubmitting(true)
  try{
    const payload={factory,trainers}
    const response:any=await smtcApi.create(payload)
    if(response?.html){
      document.open()
      document.write(response.html)
      document.close()
      return
    }
    if(response?.success!==false){
      const appId=response?.applicationId??response?.data?.applicationId
      toast.success(`SMTC registration submitted successfully!${appId?` Application ID: ${appId}`:""}`)
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
<CardTitle>SMTC Registration</CardTitle>
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

{/* STEP1 FACTORY */}

{step===1&&(

<StepCard title="Factory Details">

<TwoCol>
{Object.entries(factory).map(([k,v])=>(
<Field key={k} label={labelize(k)}>
<Input value={v}
onChange={(e)=>updateFactory(k,e.target.value)}/>
</Field>
))}
</TwoCol>

</StepCard>

)}

{/* STEP2 FACILITY */}

{step===2&&(

<StepCard title="Trainer Facility Details">

<TwoCol>

{Object.entries(facility).map(([k,v])=>(
<Field key={k} label={labelize(k)}>
{k==="trainingCenterPhoto"?(
<DocumentUploader
label=""
value={v}
onChange={(url)=>updateFacility(k,url)}
/>
):(
<Input
value={v}
onChange={(e)=>updateFacility(k,e.target.value)}
/>
)}
</Field>
))}

</TwoCol>

</StepCard>

)}

{/* STEP3 TRAINERS */}

{step===3&&(

<StepCard title="Trainer Details">

{trainers.map((trainer,ti)=>(

<div key={ti} className="border p-4 rounded-lg space-y-4">

<div className="flex justify-between">
<h4 className="font-semibold">Trainer {ti+1}</h4>

{trainers.length>1&&(
<Button
variant="destructive"
size="sm"
onClick={()=>removeTrainer(ti)}
>
Remove
</Button>
)}

</div>

<TwoCol>

<Field label="Trainer Name">
<Input value={trainer.trainerName}
onChange={(e)=>updateTrainer(ti,"trainerName",e.target.value)}/>
</Field>

<Field label="Total Years Experience">
<Input value={trainer.totalYearsExperience}
onChange={(e)=>updateTrainer(ti,"totalYearsExperience",e.target.value)}/>
</Field>

<Field label="Mobile">
<Input value={trainer.mobile}
onChange={(e)=>updateTrainer(ti,"mobile",e.target.value)}/>
</Field>



</TwoCol>

{/* EDUCATION */}

<h4 className="font-semibold">Education Details</h4>

{trainer.educationDetails.map((edu,ei)=>(

<TwoCol key={ei}>

{Object.entries(edu).map(([k,v])=>(
<Field key={k} label={labelize(k)}>
<Input
value={v}
onChange={(e)=>updateEducation(ti,ei,k,e.target.value)}
/>
</Field>
))}

</TwoCol>

))}
<TwoCol>
<Field label="Photo">
<DocumentUploader
label=""
value={trainer.photoPath}
onChange={(url)=>updateTrainer(ti,"photoPath",url)}
/>
</Field>

<Field label="Degree Document">
<DocumentUploader
label=""
value={trainer.degreeDocumentPath}
onChange={(url)=>updateTrainer(ti,"degreeDocumentPath",url)}
/>
</Field>
</TwoCol>
</div>

))}

<Button onClick={addTrainer}>
Add Trainer
</Button>

</StepCard>

)}

{/* STEP4 PREVIEW */}

{step===4&&(

<div className="bg-white border p-6 rounded-lg">

<table className="w-full border-collapse">

<PreviewHeader title="Factory Details"/>
{renderRows(factory)}

<PreviewHeader title="Trainer Facility Details"/>
{renderRows(facility)}

<PreviewHeader title="Trainer Details"/>

{trainers.map((trainer,i)=>(
<React.Fragment key={i}>

<tr>
<td colSpan={2} className="bg-gray-100 font-semibold px-3 py-2 border">
Trainer {i+1}
</td>
</tr>

{renderRows(trainer)}

</React.Fragment>
))}

</table>

</div>

)}

<div className="flex justify-between">

<Button variant="outline" onClick={prev} disabled={step===1}>
Previous
</Button>

{step<4&&(
<Button onClick={next}>
{step===3?"Preview":"Next"}
</Button>
)}

{step===4&&(
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

/* REUSABLE */

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
<td className="bg-gray-100 px-3 py-2 border w-1/3">{labelize(k)}</td>
<span className="text-sm text-gray-700">
  {v ? String(v) : "-"}
</span>
</tr>
))
}

function labelize(text:string){
return text.replace(/([A-Z])/g," $1").replace(/^./,(s)=>s.toUpperCase())
}