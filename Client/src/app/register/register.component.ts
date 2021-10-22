import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_service/account.service';
@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Input() UsersFromHomeComponent: any;
  @Output() CancelRegister = new EventEmitter();
  registerForm!: FormGroup;
  validationError: string[] = [];
  constructor(private accountService: AccountService, private toastr: ToastrService, private fb: FormBuilder,
    private router: Router) { }

  ngOnInit(): void {
    this.initializeFrom();
  }
  initializeFrom() {
    this.registerForm = this.fb.group({
      gender: ["", Validators.required],
      username: ["", Validators.required],
      knownas: ["", Validators.required],
      dateOfBirth: ["", Validators.required],
      city: ["", Validators.required],
      country: ["", Validators.required],
      password: ["", Validators.required],
      confirmpassword: ["", [Validators.required, this.matchValues("password")]]
    });
  }
  matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {
      return control?.value === control?.parent?.get(matchTo)?.value ? null : { isMatching: true };
    }
  }
  register() {
    this.accountService.register(this.registerForm.value).subscribe(res => {
      this.router.navigateByUrl('/members')
    }, err => {
      console.log(err)
      this.validationError = err
    })
  }
  cancel() {
    this.CancelRegister.emit(false);
  }

}
