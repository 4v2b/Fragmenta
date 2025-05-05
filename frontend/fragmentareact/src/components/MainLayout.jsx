import { useEffect, useState, useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { Sidebar } from '@/components/Sidebar'
import { api } from "@/api/fetchClient.js"
import { WorkspaceProvider } from '@/utils/WorkspaceContext'
import { Outlet, Navigate, useNavigate, useParams } from 'react-router'
import { logout } from '@/api/api'

import { LanguageSwitch } from "@/components/LanguageSwitch/LanguageSwitch";
import { Navbar } from "@/components/Navbar";
import { refreshToken } from "@/api/fetchClient";
import { Grid, GridItem, Stack, Button } from '@chakra-ui/react'
import { UserProvider } from '@/utils/UserContext'

export function MainLayout() {
    // TODO Normal auth verification
    const { t } = useTranslation()
    const [auth, setAuth] = useState(localStorage.getItem("accessToken") ? true : false)
    const [workspaces, setWorkspaces] = useState([])
    const { workspaceId } = useParams()
    const navigate = useNavigate()


    useEffect(() => {
        api.get("/workspaces").then(setWorkspaces)
    }, [workspaceId])

    const selectedWorkspace = useMemo(
        () => workspaces.find(w => w.id == workspaceId),
        [workspaceId, workspaces])

    const handleWorkspaceSelect = (workspaceId) => navigate(workspaceId == null ? "/" : `/workspaces/${workspaceId}`)

    console.log("selected workspace ", selectedWorkspace)

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
        <Stack justify={"stretch"} gap={0} h={"100vh"}>
            <UserProvider>
                <Navbar>
                    <LanguageSwitch />
                    <Button bg="danger" onClick={() => logout()}>{t("fields.actions.logout")}</Button>
                </Navbar>
                <Grid h={"full"} templateColumns="repeat(8, 1fr)">
                    <GridItem rowSpan={1}>
                        <Sidebar workspaces={workspaces} onWorkspaceSelect={handleWorkspaceSelect} />
                    </GridItem>
                    <GridItem colSpan={7}>
                        {workspaceId ?
                            <WorkspaceProvider role={selectedWorkspace?.role} workspaceId={selectedWorkspace?.id}>
                                <Outlet context={{ name: selectedWorkspace?.name }} />
                            </WorkspaceProvider> : <Outlet />}
                    </GridItem>
                </Grid>
            </UserProvider>
        </Stack>
        // : <>Loading</> 
        : <Navigate to="/login" replace />;
}