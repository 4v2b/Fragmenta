import { api } from "@/api/fetchClient";
import { AlertDialog } from "@/components/AlertDialog";
import { Button, Box } from "@chakra-ui/react";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import { BiTrash } from "react-icons/bi";

export function Settings() {
    const [anyWorkpace, setAnyWorkspace] = useState(false)
    const navigate = useNavigate();
    const {t} = useTranslation()


    useEffect(()=> {
        api.get("/workspaces").then(res => setAnyWorkspace(res?.some(() =>true) ) )
    }, []);

    function deleteAccount(){
        api.delete("/me").then(() => navigate("/login"))
    }

    console.log( "any workspace" , anyWorkpace)

    return <Box>
        <AlertDialog
            onConfirm={() => deleteAccount()}
            base={<Button disabled={anyWorkpace} color="danger"><BiTrash />{t("fields.actions.deleteAccount")}</Button>}
            message={t("fields.actions.areYouSureAccount")}
            title={t("fields.actions.areYouSure")}
            confirmMessage={t("fields.actions.delete")}
            cancelMessage={t("fields.actions.cancel")}
        />
    </Box>
}