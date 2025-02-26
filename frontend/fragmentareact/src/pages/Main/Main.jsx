import { useEffect, useState } from 'react'
import './Main.css'
import { useTranslation } from 'react-i18next'
import { Sidebar } from '../../components/Sidebar/Sidebar'
import { api } from "../../api/fetchClient.js"
import { WorkspaceProvider } from '@/utils/WorkspaceContext'
import { Workspace } from '../Workspace/Workspace.jsx'

export function Main() {
  const [selectedWorkspace, setSelectedWorkspace] = useState(null)
  const [workspaces, setWorkspaces] = useState([])

  const { t } = useTranslation()

  useEffect(() => {
    api.get("/workspaces").then(res => setWorkspaces(res))
  }, [])

  return (
    <div className='main-container'>
      <Sidebar workspaces={workspaces} onWorkspaceSelect={(workspace) => setSelectedWorkspace(workspace)} />
      {selectedWorkspace &&
        <WorkspaceProvider role={selectedWorkspace.role}>
          <Workspace id={selectedWorkspace.id} name={selectedWorkspace.name} />
        </WorkspaceProvider>
      }
    </div>
  )
}