import {
     Stack, Button,
    Input
} from "@chakra-ui/react"
import { NumberInput } from "@chakra-ui/react"
import { Field } from "@/components/ui/field";
import {  useState} from "react"
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
import { Checkbox } from "@chakra-ui/react"
import { useDebounce } from "@/utils/useDebounce"
import { Field as InputField } from "@chakra-ui/react"
import { useTranslation } from "react-i18next"

export function CreateStatusDialog({ onStatusCreate, statusNames }) {
    const { t, i18n } = useTranslation()
    const [selectTaskLimit, setSelectTaskLimit] = useState(false)
    const [error, setError] = useState(null)
    const [newStatus, setNewStatus] = useState({
        name: "",
        maxTasks: 1,
        colorHex: "#3182CE"
    })

    const debouncedColorChange = useDebounce((value) => {
        setNewStatus(prev => ({ ...prev, colorHex: value }));
    }, 200);

    function handleStatusCreate() {
        if (!selectTaskLimit) newStatus.maxTasks = null;
        onStatusCreate(newStatus)
    }

    return (<DialogRoot>
        <DialogTrigger asChild>
            <Button className={"status-dialog"} bg="primary">{t("fields.actions.newStatus")}</Button>
        </DialogTrigger>
        <DialogContent>
            <DialogHeader>
                <DialogTitle>{t("fields.labels.addStatus")}</DialogTitle>
            </DialogHeader>
            <DialogBody>
                <Stack spacing={4}>

                    <InputField.Root invalid={error != null}>
                        <InputField.Label>{t("fields.labels.name")}</InputField.Label>
                        <Input
                        className="status-name"
                            value={newStatus.name}
                            onChange={e => setNewStatus({ ...newStatus, name: e.target.value })} />
                        <InputField.ErrorText>{t(error)}</InputField.ErrorText>
                    </InputField.Root>


                    <Field label={t("fields.labels.taskLimit")}>

                        <Checkbox.Root className="set-limit-check" size={"sm"}>
                            <Checkbox.HiddenInput onInput={() => setSelectTaskLimit(prev => !prev)} />
                            <Checkbox.Control>
                                <Checkbox.Indicator />
                            </Checkbox.Control>
                            <Checkbox.Label >{t("fields.labels.taskLimitInfo")}</Checkbox.Label>
                        </Checkbox.Root>

                        <NumberInput.Root
                            disabled={selectTaskLimit ? false : true}
                            
                            onValueChange={e =>{
                                setNewStatus({ ...newStatus, maxTasks: e.valueAsNumber }
                                );
                            }}
                            defaultValue="0" min={1} max={50}>
                            <NumberInput.Control />
                            <NumberInput.Input className="set-limit-input" value={newStatus?.maxTasks?.toString()} />
                        </NumberInput.Root>

                    </Field>

                    <Field label={t("fields.labels.color")}>
                        <Input
                            type="color"
                            value={newStatus.colorHex}
                            onChange={e => debouncedColorChange(e.target.value)}
                        />
                    </Field>
                </Stack>
            </DialogBody>
            <DialogFooter>
                <DialogActionTrigger asChild>
                    <Button color="primary" variant="outline">{t("fields.actions.cancel")}</Button>
                </DialogActionTrigger>
                {
                    newStatus?.name == "" ?
                        (<Button className="create-status" onClick={() => setError("fields.labels.required")} bg="primary">{t("fields.actions.create")}</Button>)
                        :
                        (statusNames.includes(newStatus?.name) ?
                            (<Button className="create-status" onClick={() => setError("fields.labels.statusExists")} bg="primary">{t("fields.actions.create")}</Button>) :
                            (<DialogActionTrigger asChild>
                                <Button className="create-status" onClick={() => handleStatusCreate()} bg="primary">{t("fields.actions.create")}</Button>
                            </DialogActionTrigger>)
                        )}
            </DialogFooter>
            <DialogCloseTrigger />
        </DialogContent>
    </DialogRoot>
    )
}
