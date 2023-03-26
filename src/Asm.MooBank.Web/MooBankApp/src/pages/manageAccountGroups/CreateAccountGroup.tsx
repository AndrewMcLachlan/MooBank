import React, { useState } from "react";

import { Form, Button, InputGroup } from "react-bootstrap";

import { Page } from "../../layouts";
import { emptyGuid } from "@andrewmclachlan/mooapp";
import { useCreateAccountGroup } from "../../services";
import { AccountGroup, emptyAccountGroup } from "../../models";
import { useNavigate, useParams } from "react-router-dom";
import { AccountGroupForm } from "./AccountGroupForm";

export const CreateAccountGroup = () => (
    <AccountGroupForm accountGroup={emptyAccountGroup} />
);