import { useState } from "react";
import { useTranslation } from "react-i18next";
import { register } from "../../api/api";
import { Link, useNavigate } from "react-router";
import {
    Box,
    Button,
    Heading,
    VStack,
    Text,
    Input,
    Stack,
} from "@chakra-ui/react";
import { object } from "motion/react-client";

export function Register() {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [passwordCopy, setPasswordCopy] = useState("");
    const [name, setName] = useState("");
    const [error, setError] = useState("");
    const { t } = useTranslation();
    const { navigate } = useNavigate()

    async function handleRegister() {
        if (password !== passwordCopy) {
            setError(t("auth.errors.passwordMismatch"));
            return;
        }

        try {
            const response = await register(name, email, password);
            //console.log(response);

            if (response.status == 200) {
                navigate("/");
            }

            setError(t(response.message))
        } catch (err) {
            setError(t("auth.errors.registrationFailed"));
        }
    }

    return (
        <Box maxW="md" mx="auto" mt={10} p={8} borderWidth={1} borderRadius="lg" boxShadow="lg">
            <VStack spacing={6} align="stretch">
                <Heading size="lg" textAlign="center" mb={2}>
                    {t("auth.register")}
                </Heading>

                <Stack spacing={4}>
                    <Box>
                        <Text mb={2} fontWeight="medium">{t("fields.labels.email")}</Text>
                        <Input
                            type="email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            placeholder={t("fields.labels.email")}
                            size="md"
                        />
                    </Box>

                    <Box>
                        <Text mb={2} fontWeight="medium">{t("fields.labels.name")}</Text>
                        <Input
                            value={name}
                            onChange={(e) => setName(e.target.value)}
                            placeholder={t("fields.labels.name")}
                            size="md"
                        />
                    </Box>

                    <Box>
                        <Text mb={2} fontWeight="medium">{t("fields.labels.password")}</Text>
                        <Input
                            type="password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            placeholder={t("fields.labels.password")}
                            size="md"
                        />
                    </Box>

                    <Box>
                        <Text mb={2} fontWeight="medium">{t("fields.labels.repeatPassword")}</Text>
                        <Input
                            type="password"
                            value={passwordCopy}
                            onChange={(e) => setPasswordCopy(e.target.value)}
                            placeholder={t("fields.labels.repeatPassword")}
                            size="md"
                        />
                    </Box>
                </Stack>

                {error && <Text color="red.500" fontSize="sm">{error}</Text>}

                <Button
                    onClick={handleRegister}
                    colorScheme="blue"
                    type="submit"
                    width="full"
                    size="lg"
                    mt={2}
                >
                    {t("fields.actions.register")}
                </Button>

                <Text fontSize="sm" textAlign="center" mt={2}>
                    {t("auth.prompts.haveAccount")} <Link to="/login" color="blue.500">{t("auth.prompts.login")}</Link>
                </Text>
            </VStack>
        </Box>
    );
}