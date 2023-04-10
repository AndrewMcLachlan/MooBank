import React from "react";
import { Col, ColProps, Form, FormGroupProps } from "react-bootstrap";

export const FormGroup: React.FC<FormGroupProps & ColProps> = (props) => (
  
    <Form.Group as={Col} {...props} />
);