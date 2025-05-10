import { api } from "@/api/fetchClient";
import { AlertDialog } from "@/components/AlertDialog";
import { Button, Box, Stack, Breadcrumb, Span } from "@chakra-ui/react";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import { BiCog, BiTrash } from "react-icons/bi";
import {
    DialogActionTrigger,
    DialogBody,
    DialogCloseTrigger,
    DialogContent,
    DialogFooter,
    DialogHeader,
    DialogRoot,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog"
import { Field as InputField, Input } from "@chakra-ui/react"
import { logout } from "@/api/api";
import { LuCog, LuHouse } from "react-icons/lu";


export function Settings() {
    const [anyWorkpace, setAnyWorkspace] = useState(false)
    const navigate = useNavigate();
    const { t } = useTranslation()
    const [error, setError] = useState(false);
    const [password, setPassword] = useState("");




    useEffect(() => {
        api.get("/workspaces").then(res => setAnyWorkspace(res?.some(() => true)))
    }, []);

    function deleteAccount() {
        api.delete("/me?password=" + password).then(() => logout()).catch(error => error.message == "403" && setError(true))
    }

    return (<Stack  spacing={6} pl={8} pt={4} overflow={"auto"}>
        <Breadcrumb.Root>
            <Breadcrumb.List>
                <Breadcrumb.Item>
                    <Breadcrumb.Link href="/">
                        <LuHouse />
                        {t("common.home")}
                    </Breadcrumb.Link>
                </Breadcrumb.Item>
                <Breadcrumb.Separator />

                <Breadcrumb.Item>
                    <BiCog/> <Span fontWeight={"semibold"} p={2} >{t("fields.labels.settings")}</Span>
                </Breadcrumb.Item>
            </Breadcrumb.List>
        </Breadcrumb.Root>
        <Box>

            <DialogRoot role="alertdialog">
                <DialogTrigger asChild>
                    <Button className="delete-acc" disabled={anyWorkpace} color="danger"><BiTrash />{t("fields.actions.deleteAccount")}</Button>
                </DialogTrigger>
                <DialogContent>
                    <DialogHeader>
                        <DialogTitle>{t("fields.actions.areYouSure")}</DialogTitle>
                    </DialogHeader>
                    <DialogBody>

                        <InputField.Root invalid={error}>
                            <InputField.Label>{t("fields.labels.password")}</InputField.Label>
                            <Input
                                type="password"
                                value={password}
                                className="password"
                                onChange={e => setPassword(e.target.value)} />
                            <InputField.ErrorText>{t("auth.errors.passwordInvalid")}</InputField.ErrorText>
                        </InputField.Root>

                        <p>{t("fields.actions.areYouSureAccount")}</p>
                    </DialogBody>
                    <DialogFooter>
                        <DialogActionTrigger asChild>
                            <Button className={"alert-cancel"} variant="outline">{t("fields.actions.cancel")}</Button>
                        </DialogActionTrigger>
                        <Button disabled={password == ""} onClick={() => deleteAccount()} className={"alert-confirm"} bg="danger">{t("fields.actions.delete")}</Button>
                    </DialogFooter>
                    <DialogCloseTrigger />
                </DialogContent>
            </DialogRoot>
        </Box>
    </Stack>);
}