import { useTranslation } from "react-i18next";
import { Toaster, toaster } from "@/components/ui/toaster";
import { Box, Button, Code, FileUpload, Icon, InputGroup, useFileUpload } from "@chakra-ui/react"
import { LuUpload } from "react-icons/lu"

const maxFiles = 10;
const maxFileSize = 10485760; // 10MB

export function FileManager({ allowedTypes }) {
    const { t } = useTranslation();

    function validateFileType(file, details) {
        if (["docx", "txt"].some(e => file.name.endsWith(e))) {
            details.acceptedFiles = [...details.acceptedFiles, file];
            return null;
        }

        return ["forbiddenFileType"];
    }

    function fileRejected(details) {
        const errorType = details.files[0].errors[0] === "FILE_TOO_LARGE" ? "fileTooLarge" : details.files[0]?.errors[0];

        toaster.create({
            title: t(`errors.${errorType}`, { filename: details.files[0]?.file.name }),
            type: "error",
        });
    }

    async function handleUpload(files) {
        if (!files.length) return;

        console.log(files)

        const formData = new FormData();
        files.forEach((file) => formData.append("files", file));

        console.log([...formData.entries()]);

        // try {
        //     const response = await fetch("/upload", {
        //         method: "POST",
        //         body: formData,
        //     });

        //     if (!response.ok) throw new Error("Upload failed");

        //     toaster.create({ title: t("fields.labels.uploadSuccess"), type: "success" });
        // } catch (error) {
        //     toaster.create({ title: t("fields.labels.uploadError"), type: "error" });
        // }
    }

    return (
        <FileUpload.Root onFileReject={fileRejected} validate={validateFileType} maxFileSize={maxFileSize} maxFiles={maxFiles} maxW="xl">
            <Toaster />
            <FileUpload.HiddenInput />
            <FileUpload.Dropzone>
                <Icon size="md" color="fg.muted">
                    <LuUpload />
                </Icon>
                <FileUpload.DropzoneContent>
                    <Box>{t("fields.labels.dragFile")}</Box>
                    <Box color="fg.muted">{t("fields.labels.fileConstraint")}</Box>
                </FileUpload.DropzoneContent>
            </FileUpload.Dropzone>
            <FileUpload.ItemGroup>
                <FileUpload.Context>
                    {({ acceptedFiles }) => (
                        <>
                            {acceptedFiles.map((file) => (
                                <FileUpload.Item key={file.name} file={file}>
                                    <FileUpload.ItemPreview />
                                    <FileUpload.ItemName />
                                    <FileUpload.ItemSizeText />
                                    <FileUpload.ItemDeleteTrigger />
                                </FileUpload.Item>
                            ))}
                            <Button mt={3} colorScheme="blue" onClick={() => handleUpload(acceptedFiles)} isDisabled={!acceptedFiles.length}>
                                {t("fields.labels.upload")}
                            </Button>
                        </>
                    )}
                </FileUpload.Context>
            </FileUpload.ItemGroup>
        </FileUpload.Root>
    );
}