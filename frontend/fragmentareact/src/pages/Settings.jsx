import { api } from "@/api/fetchClient";
import { AlertDialog } from "@/components/AlertDialog";
import { Button, Box, Stack, Breadcrumb, Span, Editable, Text, Field } from "@chakra-ui/react";
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
import { useUser } from "@/utils/UserContext";
import {
    PasswordInput
} from "@/components/ui/password-input"
import { Toaster, toaster } from "@/components/ui/toaster";


export function Settings() {
    const [anyWorkpace, setAnyWorkspace] = useState(false)
    const navigate = useNavigate();
    const { t } = useTranslation()
    const [error, setError] = useState(false)
    const [password, setPassword] = useState("");
    const { userName, updateName } = useUser();
    const [newName, setNewName] = useState("")
    const [passwordForm, setPasswordForm] = useState({ oldPassword: "", newPassword: "" })
    const [errors, setErrors] = useState({})

    useEffect(() => {
        if (userName) setNewName(userName);
    }, [userName]);

    useEffect(() => {
        api.get("/workspaces").then(res => setAnyWorkspace(res?.some(() => true)))
    }, []);

    function deleteAccount() {
        api.delete("/me?password=" + password).then(() => logout()).catch(error => error.message == "403" && setError(true))
    }

    function updateUserName() {
        api.post("/me/name?newName=" + newName, {}).then(_ => updateName(newName))
    }

    function changePassword() {
        api.post("/me/password", passwordForm).then(_ => {
            setErrors({})
            toaster.create({
                title: t("auth.success.resetPassword"),
                type: "success",
            })
            setPasswordForm({ oldPassword: "", newPassword: "" });
        }).catch(error => error.status == 400 && setErrors(error.errors))
    }

    return (<Stack spacing={6} pl={8} pr={8} pt={4} overflow={"auto"} >
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
                    <BiCog /> <Span fontWeight={"semibold"} p={2} >{t("fields.labels.settings")}</Span>
                </Breadcrumb.Item>
            </Breadcrumb.List>
        </Breadcrumb.Root>
        <Box bg={"background"} p={8} borderRadius={3}>
            <Toaster />

            {userName && <Stack w={"fit-content"} mb={6}>
                <Text fontWeight={"semibold"}>{t("fields.labels.changeName")}</Text>
                <Editable.Root

                    maxLength={100}
                    textAlign="start"
                    defaultValue={userName}
                    value={newName}
                    onValueChange={(e) => setNewName(e.value)}
                >
                    <Editable.Preview w={"fit-content"} />
                    <Editable.Input />
                </Editable.Root>
                <Button onClick={() => updateUserName()} disabled={newName == userName || newName == ''}>{t("fields.actions.save")}</Button>
            </Stack>}

            <Stack w={"fit-content"} mb={6}>
                <Text fontWeight={"semibold"}>{t("fields.labels.changePassword")}</Text>

                <Field.Root invalid={errors?.OldPassword ? true : false}>
                    <Field.Label>{t("fields.labels.oldPassword")}</Field.Label>
                    <PasswordInput value={passwordForm.oldPassword} onChange={(e) => setPasswordForm({ ...passwordForm, oldPassword: e.target.value })} />
                    <Field.ErrorText>{t(errors?.OldPassword)}</Field.ErrorText>
                </Field.Root>


                <Field.Root invalid={errors?.NewPassword ? true : false}>
                    <Field.Label>{t("fields.labels.newPassword")}</Field.Label>
                    <PasswordInput value={passwordForm.newPassword} onChange={(e) => setPasswordForm({ ...passwordForm, newPassword: e.target.value })} />
                    <Field.ErrorText>{t(errors?.NewPassword)}</Field.ErrorText>
                    <Field.HelperText>{t("fields.labels.passwordHelp")}</Field.HelperText>
                </Field.Root>

                <Button onClick={() => changePassword()} disabled={passwordForm.oldPassword == '' || passwordForm.newPassword.length == ''}>{t("fields.actions.save")}</Button>
            </Stack>

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

            {anyWorkpace && <Text fontSize={12} color="danger">{t("common.accountDeleteHint")}</Text> }
        </Box>
    </Stack>);
}