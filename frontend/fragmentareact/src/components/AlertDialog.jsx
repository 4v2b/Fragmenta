import { Button } from "@chakra-ui/react"
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
                    <Button  className={"alert-cancel"} variant="outline">{cancelMessage}</Button>
                </DialogActionTrigger>
                <Button onClick={onConfirm} className={"alert-confirm"} bg="danger">{confirmMessage}</Button>
            </DialogFooter>
            <DialogCloseTrigger />
        </DialogContent>
    </DialogRoot>
}