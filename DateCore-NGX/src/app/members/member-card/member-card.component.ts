import { Component, OnInit, Input } from '@angular/core';
import { User } from 'src/app/_models/User';
import { AuthService } from 'src/app/_services/auth.service';
import { UserService } from 'src/app/_services/user.service';
import { AlertifyService } from 'src/app/_services/alertify.service';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css']
})
export class MemberCardComponent implements OnInit {

  @Input() user: User;

  constructor(private authService: AuthService, private userService: UserService, private alertif: AlertifyService) { }

  ngOnInit() {
  }

  sendLike(recipientId: string) {
    this.userService.sendLike(this.authService.decodedToken.nameid, recipientId).subscribe(data => {
      this.alertif.success('You have liked: ' + this.user.knownAs);
    }, error => {
      this.alertif.error(error);
    });
  }

}
