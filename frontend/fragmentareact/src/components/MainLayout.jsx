import { useEffect, useState, useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { Sidebar } from '@/components/Sidebar'
import { api } from "@/api/fetchClient.js"
import { WorkspaceProvider } from '@/utils/WorkspaceContext'
import { Outlet, Navigate, useNavigate, useParams } from 'react-router'
import { useAuth } from '@/utils/useAuth'
import { logout } from '@/api/api'

import { LanguageSwitch } from "@/components/LanguageSwitch/LanguageSwitch";
import { Navbar } from "@/components/Navbar";
import { refreshToken } from "@/api/fetchClient";
import { Grid, GridItem, Stack, HStack, Button } from '@chakra-ui/react'

export function MainLayout() {
    // TODO Normal auth verification
    const { t } = useTranslation()
    const [auth, setAuth] = useState(localStorage.getItem("accessToken") ? true : false)
    const [workspaces, setWorkspaces] = useState([])
    const { workspaceId } = useParams()
    const navigate = useNavigate()

    useEffect(() => {
        api.get("/workspaces").then(setWorkspaces)
    }, [])

    const selectedWorkspace = useMemo(
        () => workspaces.find(w => w.id == workspaceId),
        [workspaceId, workspaces])

    const handleWorkspaceSelect = (workspace) => navigate(`/workspaces/${workspace?.id}`)

    useEffect(() => {

        async function refresh() {
            await refreshToken()

            if (localStorage.getItem("accessToken")) {
                setAuth(true)
            }
            else {
                console.log("Cannot refresh token")
            }
        }

        if (!auth) {
            refresh()
        }

    }, [auth])

    return auth
        ?
        <Stack gap={0}>
            <Navbar>
                <LanguageSwitch />
                <Button bg="danger" onClick={() => logout()}>{t("fields.actions.logout")}</Button>
            </Navbar>
            <Grid h={"full"} templateColumns="repeat(6, 1fr)">
                <GridItem rowSpan={1}>
                    <Sidebar workspaces={workspaces} onWorkspaceSelect={handleWorkspaceSelect} />
                </GridItem>
                <GridItem colSpan={5}>
                    {workspaceId ?
                        <WorkspaceProvider role={selectedWorkspace?.role} workspaceId={selectedWorkspace?.id}>
                            <Outlet context={{ name: selectedWorkspace?.name }} />
                        </WorkspaceProvider> : <Outlet />}
                </GridItem>
            </Grid>
        </Stack>
        // : <>Loading</> 
        : <Navigate to="/login" replace />;
}