import { useEffect, useState, useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { Sidebar } from '@/components/Sidebar/Sidebar'
import { api } from "@/api/fetchClient.js"
import { WorkspaceProvider } from '@/utils/WorkspaceContext'
import { Outlet, useNavigate, useParams } from 'react-router'

export function MainLayout() {
    const [workspaces, setWorkspaces] = useState([])
    const { workspaceId } = useParams()
    const navigate = useNavigate()
    const { t } = useTranslation()

    useEffect(() => {
        api.get("/workspaces").then(setWorkspaces)
        console.log("load workspaces")
    }, [])

    // Memoize workspace lookup to prevent unnecessary recalculations
    const selectedWorkspace = useMemo(
        () => workspaces.find(w => w.id == workspaceId),
        [workspaceId, workspaces])

    console.log(selectedWorkspace, workspaceId, workspaces)

    const handleWorkspaceSelect = (workspace) => navigate(`/workspaces/${workspace.id}`)

    return (
        <div className='main-container'>
            <Sidebar workspaces={workspaces} onWorkspaceSelect={handleWorkspaceSelect} />
            {workspaceId ?
                <WorkspaceProvider role={selectedWorkspace?.role} workspaceId={selectedWorkspace?.id}>
                    <Outlet context={{ name : selectedWorkspace?.name }} />
                </WorkspaceProvider> : <Outlet />}
        </div>
    )
}