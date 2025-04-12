import { Flex, HStack } from "@chakra-ui/react"

export function Navbar({ children }) {
    return (<Flex bg={"background"} p={2} gap="4" align="center" justify="flex-end">
        {children}
    </Flex>);
}