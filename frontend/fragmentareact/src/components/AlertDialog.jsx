import { useEffect, useState } from "react"
import { api } from "@/api/fetchClient"
import { useTranslation } from "react-i18next"
import { Stack, Button, Wrap, Badge, CloseButton, Table } from "@chakra-ui/react"
import { useWorkspace } from "@/utils/WorkspaceContext"
import { Autocomplete } from "@/components/Autocomplete"
import { canDeleteMember } from "@/utils/permissions"
import { LiaDoorOpenSolid } from "react-icons/lia";
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

export function AlertDialog({ base, title, message, cancelMessage, confirmMessage, onConfirm }) {
   
    return <DialogRoot role="alertdialog">
        <DialogTrigger asChild>
            {base}
        </DialogTrigger>
        <DialogContent>
            <DialogHeader>
                <DialogTitle>{title}</DialogTitle>
            </DialogHeader>
            <DialogBody>
                <p>{message}</p>
            </DialogBody>
            <DialogFooter>
                <DialogActionTrigger asChild>
                    <Button variant="outline">{cancelMessage}</Button>
                </DialogActionTrigger>
                <Button onClick={onConfirm} colorPalette="red">{confirmMessage}</Button>
            </DialogFooter>
            <DialogCloseTrigger />
        </DialogContent>
    </DialogRoot>
}